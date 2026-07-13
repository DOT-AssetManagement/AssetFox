<template>
    <v-dialog v-model="dialogVisible" max-width="780" persistent>
        <v-card class="share-report-template">
            <v-card-title class="share-report-template__title">
                <div>
                    <div class="share-report-template__heading">Share Report Template</div>
                    <div class="share-report-template__name">{{ schema?.name || '' }}</div>
                </div>
                <v-spacer />
                <v-btn icon size="small" variant="text" @click="close">
                    <i class="fas fa-times" />
                </v-btn>
            </v-card-title>

            <v-divider />

            <v-card-text class="share-report-template__body">
                <div class="share-report-template__all-row">
                    <v-switch
                        v-model="shareWithAll"
                        color="#2A578D"
                        density="compact"
                        hide-details
                        label="Share with all"
                    />
                    <span
                        class="share-report-template__pill"
                        :class="{ 'share-report-template__pill--on': shareWithAll }"
                    >
                        {{ shareWithAll ? 'On' : 'Off' }}
                    </span>
                </div>

                <div class="share-report-template__add-row">
                    <v-autocomplete
                        v-model="selectedUserId"
                        :items="availableUsers"
                        item-title="username"
                        item-value="id"
                        density="compact"
                        variant="outlined"
                        hide-details
                        clearable
                        label="Add user"
                        class="share-report-template__user-picker"
                    />
                    <v-btn
                        variant="flat"
                        color="#2A578D"
                        class="text-none share-report-template__add-btn"
                        :disabled="!selectedUserId"
                        @click="addSelectedUser"
                    >
                        <i class="fas fa-plus mr-2" />
                        Add
                    </v-btn>
                </div>

                <v-data-table
                    :headers="headers"
                    :items="rows"
                    :items-per-page="5"
                    :items-per-page-options="pageSizeOptions"
                    item-key="userId"
                    density="compact"
                    class="ghd-table share-report-template__table"
                >
                    <template v-slot:item="{ item }">
                        <tr>
                            <td>
                                <div class="share-report-template__user">
                                    <i class="fas fa-user" />
                                    <span>{{ item.userName }}</span>
                                </div>
                            </td>
                            <td>
                                <span
                                    v-if="item.accessLevel === ownerAccess"
                                    class="share-report-template__owner"
                                >
                                    Owner
                                </span>
                                <v-select
                                    v-else
                                    v-model="item.accessLevel"
                                    :items="accessOptions"
                                    item-title="title"
                                    item-value="value"
                                    density="compact"
                                    variant="outlined"
                                    hide-details
                                    class="share-report-template__access-select"
                                />
                            </td>
                            <td>
                                <v-btn
                                    v-if="item.accessLevel !== ownerAccess"
                                    icon
                                    size="x-small"
                                    variant="text"
                                    class="share-report-template__remove"
                                    title="Remove user"
                                    @click="removeUser(item.userId)"
                                >
                                    <i class="fas fa-trash" />
                                </v-btn>
                            </td>
                        </tr>
                    </template>

                    <template v-slot:no-data>
                        <div class="share-report-template__empty">No individual users selected</div>
                    </template>
                </v-data-table>
            </v-card-text>

            <v-divider />

            <v-card-actions class="share-report-template__actions">
                <v-spacer />
                <v-btn variant="text" class="text-none" @click="close">Cancel</v-btn>
                <v-btn
                    variant="flat"
                    color="#2A578D"
                    class="text-none"
                    :loading="saving"
                    @click="save"
                >
                    Save Sharing
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { ReportSchemaDTO } from '@/shared/models/iAM/report-schema';
import { LibraryUser, User } from '@/shared/models/iAM/user';
import { useReportSchemaStore, useUserStore } from '@/stores';

const props = defineProps<{
    modelValue: boolean;
    schema: ReportSchemaDTO | null;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'saved', schema: ReportSchemaDTO): void;
}>();

const reportSchemaStore = useReportSchemaStore();
const userStore = useUserStore();

const readAccess = 0;
const modifyAccess = 1;
const ownerAccess = 2;

const dialogVisible = computed({
    get: () => props.modelValue,
    set: (value: boolean) => emit('update:modelValue', value),
});

const shareWithAll = ref(false);
const rows = ref<LibraryUser[]>([]);
const selectedUserId = ref<string | null>(null);
const saving = ref(false);

const headers = [
    { title: 'User', key: 'userName', sortable: true },
    { title: 'Access', key: 'accessLevel', sortable: false },
    { title: '', key: 'actions', sortable: false, width: '56px' },
];

const pageSizeOptions = [
    { value: 5, title: '5' },
    { value: 10, title: '10' },
    { value: 25, title: '25' },
];

const accessOptions = [
    { title: 'Can view', value: readAccess },
    { title: 'Can edit', value: modifyAccess },
];

const allUsers = computed<User[]>(() => userStore.getUsers || []);
const availableUsers = computed<User[]>(() => {
    const selectedIds = new Set(rows.value.map((row) => row.userId));
    return allUsers.value
        .filter((user) => user.activeStatus !== false)
        .filter((user) => !selectedIds.has(user.id))
        .sort((a, b) => a.username.localeCompare(b.username));
});

watch(
    () => props.modelValue,
    (visible) => {
        if (visible) {
            void loadSharing();
        }
    },
);

async function loadSharing() {
    if (!props.schema?.id) {
        return;
    }

    shareWithAll.value = props.schema.isSharedWithAll;
    selectedUserId.value = null;

    if (allUsers.value.length === 0) {
        await userStore.getAllUsers();
    }

    const users = await reportSchemaStore.getReportSchemaUsers(props.schema.id) as LibraryUser[];
    rows.value = normalizeRows(users || props.schema.users || []);
}

function normalizeRows(users: LibraryUser[]): LibraryUser[] {
    const byUserId = new Map<string, LibraryUser>();
    users.forEach((user) => {
        if (!user?.userId) {
            return;
        }
        const existing = byUserId.get(user.userId);
        if (!existing || user.accessLevel > existing.accessLevel) {
            byUserId.set(user.userId, { ...user });
        }
    });

    return Array.from(byUserId.values())
        .sort((a, b) => {
            if (a.accessLevel === ownerAccess && b.accessLevel !== ownerAccess) {
                return -1;
            }
            if (a.accessLevel !== ownerAccess && b.accessLevel === ownerAccess) {
                return 1;
            }
            return (a.userName || '').localeCompare(b.userName || '');
        });
}

function addSelectedUser() {
    if (!selectedUserId.value) {
        return;
    }

    const user = allUsers.value.find((candidate) => candidate.id === selectedUserId.value);
    if (!user) {
        return;
    }

    rows.value = normalizeRows([
        ...rows.value,
        {
            userId: user.id,
            userName: user.username,
            accessLevel: readAccess,
        },
    ]);
    selectedUserId.value = null;
}

function removeUser(userId: string) {
    rows.value = rows.value.filter((row) => row.userId !== userId || row.accessLevel === ownerAccess);
}

async function save() {
    if (!props.schema?.id) {
        return;
    }

    saving.value = true;
    try {
        const saved = await reportSchemaStore.updateReportSchemaSharing(
            props.schema.id,
            shareWithAll.value,
            normalizeRows(rows.value),
        ) as ReportSchemaDTO | null;

        if (saved) {
            emit('saved', saved);
        }
        close();
    } finally {
        saving.value = false;
    }
}

function close() {
    dialogVisible.value = false;
}
</script>

<style scoped>
.share-report-template {
    border-radius: 8px !important;
}

.share-report-template__title {
    display: flex;
    align-items: center;
    padding: 16px 18px;
}

.share-report-template__heading {
    color: #24364B;
    font-size: 16px;
    font-weight: 700;
}

.share-report-template__name {
    color: #6F7F91;
    font-size: 12px;
    margin-top: 2px;
}

.share-report-template__body {
    padding: 16px 18px 10px 18px;
}

.share-report-template__all-row {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 12px;
    border: 1px solid #DDE6F1;
    border-radius: 8px;
    background: #F7F9FC;
    margin-bottom: 14px;
}

.share-report-template__pill {
    border-radius: 999px;
    background: #EEF2F6;
    color: #526071;
    font-size: 11px;
    font-weight: 700;
    padding: 2px 9px;
}

.share-report-template__pill--on {
    background: #E4F4EA;
    color: #246044;
}

.share-report-template__add-row {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-bottom: 12px;
}

.share-report-template__user-picker {
    flex: 1;
}

.share-report-template__add-btn {
    border-radius: 5px !important;
    letter-spacing: 0 !important;
    min-height: 40px;
}

.share-report-template__table {
    border: 1px solid #E5ECF4;
    border-radius: 8px;
    overflow: hidden;
}

.share-report-template__user {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #24364B;
    font-size: 13px;
    font-weight: 600;
}

.share-report-template__user i {
    color: #6F7F91;
    font-size: 11px;
}

.share-report-template__owner {
    display: inline-flex;
    align-items: center;
    min-height: 24px;
    border-radius: 999px;
    padding: 2px 10px;
    background: #E7F0FA;
    color: #24527F;
    font-size: 11px;
    font-weight: 700;
}

.share-report-template__access-select {
    max-width: 180px;
}

.share-report-template__remove {
    color: #B42318 !important;
}

.share-report-template__empty {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 80px;
    color: #7A8696;
    font-size: 13px;
}

.share-report-template__actions {
    padding: 12px 18px;
}

@media (max-width: 720px) {
    .share-report-template__add-row {
        align-items: stretch;
        flex-direction: column;
    }

    .share-report-template__access-select {
        max-width: none;
    }
}
</style>
