import {
    HttpTransportType,
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel,
} from '@microsoft/signalr';
import { watch } from 'vue';
import { SimulationAnalysisDetail } from '@/shared/models/iAM/simulation-analysis-detail';
import { SimulationReportDetail } from '@/shared/models/iAM/simulation-report-detail';
import { NetworkRollupDetail } from '@/shared/models/iAM/network-rollup-detail';
import { hasValue } from '@/shared/utils/has-value-util';
import { getUserName } from '@/shared/utils/get-user-info';
import pinia, { useAuthenticationStore } from '@/stores';
import { queuedWorkStatusUpdate } from './shared/models/iAM/queuedWorkStatusUpdate';
import { importCompletion } from './shared/models/iAM/ImportCompletion';
import mitt from 'mitt';
import { WorkType } from './shared/models/iAM/scenario';

export default {
    install: (app: any, options: any) => {
        const connection = new HubConnectionBuilder()
            .withUrl(
                `${import.meta.env.VITE_APP_BRIDGECARE_CORE_URL}/bridgecarehub/`,
                {
                    transport: HttpTransportType.LongPolling,
                },
            )
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        const emitter = mitt()

        connection.on(
            Hub.BroadcastType.BroadcastAssignDataStatus,
            (networkRollupDetail: NetworkRollupDetail, percentage) => {
                emitter.emit(
                    Hub.BroadcastEventType.BroadcastAssignDataStatusEvent,
                    { networkRollupDetail, percentage },
                );
            },
        );

        connection.on(
            Hub.BroadcastType.BroadcastReportGenerationStatus,
            (simulationReportDetail: SimulationReportDetail) => {
                emitter.emit(
                    Hub.BroadcastEventType
                        .BroadcastReportGenerationStatusEvent,
                    { simulationReportDetail },
                );
            },
        );

        connection.on(Hub.BroadcastType.BroadcastDataMigration, status => {
            emitter.emit(
                Hub.BroadcastEventType.BroadcastDataMigrationEvent,
                { status },
            );
        });

        connection.on(
            Hub.BroadcastType.BroadcastScenarioStatusUpdate,
            (status, scenarioId) => {
                emitter.emit(
                    Hub.BroadcastEventType.BroadcastScenarioStatusUpdateEvent,
                    { status, scenarioId },
                );
            },
        );

        connection.on(
            Hub.BroadcastType.BroadcastSimulationAnalysisDetail,
            (simulationAnalysisDetail: SimulationAnalysisDetail) => {
                emitter.emit(
                    Hub.BroadcastEventType
                        .BroadcastSimulationAnalysisDetailEvent,
                    {simulationAnalysisDetail},
                );
            },
        );

        connection.on(Hub.BroadcastType.BroadcastWorkQueueUpdate, workId => {
            emitter.emit(Hub.BroadcastEventType.BroadcastWorkQueueUpdateEvent, {
                workId,
            });
        });

        connection.on(Hub.BroadcastType.BroadcastFastWorkQueueUpdate, workId => {
            emitter.emit(Hub.BroadcastEventType.BroadcastFastWorkQueueUpdateEvent, {
                workId,
            });
        });

        connection.on(Hub.BroadcastType.BroadcastWorkQueueStatusUpdate, (queueItem: queuedWorkStatusUpdate) => {
            emitter.emit(Hub.BroadcastEventType.BroadcastWorkQueueStatusUpdateEvent, {
                queueItem,
            });
        });   
        
        connection.on(Hub.BroadcastType.BroadcastFastWorkQueueStatusUpdate, (queueItem: queuedWorkStatusUpdate) => {
            emitter.emit(Hub.BroadcastEventType.BroadcastFastWorkQueueStatusUpdateEvent, {
                queueItem,
            });
        });

        connection.on(Hub.BroadcastType.BroadcastImportCompletion, (importComp: importCompletion) => {
            emitter.emit(Hub.BroadcastEventType.BroadcastImportCompletionEvent, {
                importComp,
            });
        });

        connection.on(Hub.BroadcastType.BroadcastImportFailure, () => {
            emitter.emit(Hub.BroadcastEventType.BroadcastImportFailureEvent, {                
            });
        });

        connection.on(Hub.BroadcastType.BroadcastSimulationDeletionCompletion, (workType: WorkType) => {
            emitter.emit(Hub.BroadcastEventType.BroadcastSimulationDeletionCompletionEvent, {
                workType
            });
        });

        connection.on(Hub.BroadcastType.BroadcastSimulationOutputDeletionCompletion, (workType: WorkType) => {
            emitter.emit(Hub.BroadcastEventType.BroadcastSimulationOutputDeletionCompletionEvent, {
                workType
            });
        });

        connection.on(Hub.BroadcastType.BroadcastError, (error, stackTrace) => {
            emitter.emit(Hub.BroadcastEventType.BroadcastErrorEvent, {
                error, stackTrace
            });
        });

        connection.on(Hub.BroadcastType.BroadcastWarning, warning => {
            emitter.emit(Hub.BroadcastEventType.BroadcastWarningEvent, {
                warning,
            });
        });

        connection.on(Hub.BroadcastType.BroadcastInfo, info => {
            emitter.emit(Hub.BroadcastEventType.BroadcastInfoEvent, {
                info,
            });
        });
        
        connection.on(Hub.BroadcastType.BroadcastTaskCompleted, task => {
            emitter.emit(Hub.BroadcastEventType.BroadcastTaskCompletedEvent, {
                task,
            });
        });
        app.provide('emitter', emitter);
        const authenticationStore = useAuthenticationStore(pinia);
        let startPromise: Promise<void> | null = null;
        let associationPromise: Promise<void> | null = null;
        let associatedUsername = '';
        let hasConnected = false;
        let startRetryTimer: ReturnType<typeof setTimeout> | null = null;
        let associationRetryTimer: ReturnType<typeof setTimeout> | null = null;

        function getCurrentUsername(): string {
            const storeUsername = authenticationStore.username?.trim();
            if (hasValue(storeUsername)) {
                return storeUsername;
            }

            if (
                !hasValue(localStorage.getItem('UserInfo')) &&
                !hasValue(localStorage.getItem('LoggedInUser'))
            ) {
                return '';
            }

            try {
                return getUserName()?.trim() ?? '';
            } catch (err) {
                console.error('Failed to determine the hub username', err);
                return '';
            }
        }

        function scheduleAssociationRetry() {
            if (associationRetryTimer !== null) {
                return;
            }

            associationRetryTimer = setTimeout(() => {
                associationRetryTimer = null;
                void synchronizeUserAssociation();
            }, 5000);
        }

        async function synchronizeUserAssociation(): Promise<void> {
            if (connection.state !== HubConnectionState.Connected) {
                return;
            }

            if (associationPromise !== null) {
                await associationPromise.catch(() => undefined);
                return;
            }

            associationPromise = (async () => {
                const username = getCurrentUsername();
                if (username === associatedUsername) {
                    return;
                }

                if (hasValue(associatedUsername)) {
                    await connection.invoke(
                        'DisassociateMessage',
                        associatedUsername,
                    );
                    associatedUsername = '';
                }

                if (!hasValue(username)) {
                    return;
                }

                await connection.invoke('AssociateMessage', username);
                associatedUsername = username;
            })();

            try {
                await associationPromise;
            } catch (err) {
                console.error('Failed to associate the hub connection', err);
                scheduleAssociationRetry();
            } finally {
                associationPromise = null;
            }

            if (
                connection.state === HubConnectionState.Connected &&
                getCurrentUsername() !== associatedUsername
            ) {
                scheduleAssociationRetry();
            }
        }

        function scheduleStart() {
            if (startRetryTimer !== null) {
                return;
            }

            startRetryTimer = setTimeout(() => {
                startRetryTimer = null;
                void start();
            }, 5000);
        }

        function emitConnectionRestored() {
            emitter.emit(
                Hub.BroadcastEventType.BroadcastConnectionRestoredEvent,
            );
            emitter.emit(
                Hub.BroadcastEventType.BroadcastWorkQueueUpdateEvent,
                { workId: null },
            );
            emitter.emit(
                Hub.BroadcastEventType.BroadcastFastWorkQueueUpdateEvent,
                { workId: null },
            );
        }

        function start(): Promise<void> {
            if (connection.state === HubConnectionState.Connected) {
                return synchronizeUserAssociation();
            }

            if (
                connection.state !== HubConnectionState.Disconnected ||
                startPromise !== null
            ) {
                return startPromise ?? Promise.resolve();
            }

            const isRecovery = hasConnected;
            startPromise = connection
                .start()
                .then(() => synchronizeUserAssociation())
                .then(() => {
                    hasConnected = true;
                    if (isRecovery) {
                        emitConnectionRestored();
                    }
                })
                .catch((err: any) => {
                    console.error('Failed to connect with hub', err);
                    scheduleStart();
                })
                .finally(() => {
                    startPromise = null;
                });

            return startPromise;
        }

        connection.onreconnecting(err => {
            associatedUsername = '';
            console.warn('Hub connection interrupted; reconnecting', err);
        });

        connection.onreconnected(() => {
            associatedUsername = '';
            void synchronizeUserAssociation().then(() => {
                hasConnected = true;
                emitConnectionRestored();
            });
        });

        connection.onclose(err => {
            associatedUsername = '';
            console.error('Hub connection closed', err);
            scheduleStart();
        });

        watch(
            () => [
                authenticationStore.username,
                authenticationStore.authenticated,
                authenticationStore.securityType,
            ],
            () => void synchronizeUserAssociation(),
        );

        void start();
    },
};

export const Hub = {
    BroadcastType: {
        BroadcastError: 'BroadcastError',
        BroadcastInfo: 'BroadcastInfo',
        BroadcastWarning: 'BroadcastWarning',
        BroadcastTaskCompleted: 'BroadcastTaskCompleted',
        BroadcastAssignDataStatus: 'BroadcastAssignDataStatus',
        BroadcastReportGenerationStatus:
            'BroadcastReportGenerationStatus',
        BroadcastScenarioStatusUpdate: 'BroadcastScenarioStatusUpdate',
        BroadcastSimulationAnalysisDetail: 'BroadcastSimulationAnalysisDetail',
        BroadcastDataMigration: 'BroadcastDataMigration',
        BroadcastNetworkRollupDetail: 'BroadcastNetworkRollupDetail',
        BroadcastWorkQueueUpdate: 'BroadcastWorkQueueUpdate',
        BroadcastWorkQueueStatusUpdate: 'BroadcastWorkQueueStatusUpdate',
        BroadcastFastWorkQueueUpdate: 'BroadcastFastWorkQueueUpdate',
        BroadcastFastWorkQueueStatusUpdate: 'BroadcastFastWorkQueueStatusUpdate',
        BroadcastImportCompletion: 'BroadcastImportCompletion', 
        BroadcastImportFailure: 'BroadcastImportFailure',  
        BroadcastSimulationDeletionCompletion: 'BroadcastSimulationDeletionCompletion',
        BroadcastSimulationOutputDeletionCompletion: 'BroadcastSimulationOutputDeletionCompletion'     
    },
    BroadcastEventType: {
        BroadcastErrorEvent: 'BroadcastErrorEvent',
        BroadcastWarningEvent: 'BroadcastWarningEvent',
        BroadcastInfoEvent: 'BroadcastInfoEvent',
        BroadcastTaskCompletedEvent: 'BroadcastTaskCompleted',
        BroadcastAssignDataStatusEvent: 'BroadcastAssignDataStatusEvent',
        BroadcastReportGenerationStatusEvent:
            'BroadcastReportGenerationStatusEvent',
        BroadcastScenarioStatusUpdateEvent:
            'BroadcastScenarioStatusUpdateEvent',
        BroadcastSimulationAnalysisDetailEvent:
            'BroadcastSimulationAnalysisDetailEvent',
        BroadcastDataMigrationEvent: 'BroadcastDataMigrationEvent',
        BroadcastNetworkRollupDetailEvent: 'BroadcastNetworkRollupDetailEvent',
        BroadcastWorkQueueUpdateEvent: 'BroadcastWorkQueueUpdateEvent',
        BroadcastWorkQueueStatusUpdateEvent: 'BroadcastWorkQueueStatusUpdateEvent',
        BroadcastFastWorkQueueUpdateEvent: 'BroadcastFastWorkQueueUpdateEvent',
        BroadcastFastWorkQueueStatusUpdateEvent: 'BroadcastFastWorkQueueStatusUpdateEvent',        
        BroadcastConnectionRestoredEvent: 'BroadcastConnectionRestoredEvent',
        BroadcastImportCompletionEvent: 'BroadcastImportCompletionEvent',
        BroadcastImportFailureEvent: 'BroadcastImportFailureEvent',
        BroadcastSimulationDeletionCompletionEvent: 'BroadcastSimulationDeletionCompletionEvent',
        BroadcastSimulationOutputDeletionCompletionEvent: 'BroadcastSimulationOutputDeletionCompletionEvent'
    },
};
