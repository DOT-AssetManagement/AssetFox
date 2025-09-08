<template>
  <v-row>
    <v-dialog style='max-width: 900px; max-height: 650px' persistent scrollable v-model ='dialogData.showDialog'>
      <v-card>
        <v-card-title class="ghd-dialog-box-padding-top">
          <v-row justify-space-between align-center>
              <div class="ghd-control-dialog-header">Edit Data Source Mappings</div>
              <v-spacer></v-spacer>
              <XButton @click="onSubmit(false)"/>
          </v-row>
        </v-card-title>
        <div style='height: 600px; max-width:900px; margin-top:20px;' class="ghd-dialog-box-padding-center">
          <div style='max-height: 500px; overflow-y:auto;'>
            <v-data-table-server
                      id="EditBudgetsDialog-budgets-dataTable"
                      :headers='gridHeaders'
                      :items="editDataSourceMappingsGridData"
                      :items-length="editDataSourceMappingsGridData.length"
                      sort-asc-icon="custom:GhdTableSortAscSvg"
                      sort-desc-icon="custom:GhdTableSortDescSvg"
                      hide-actions
                      item-key='id'
                      v-model='editDataSourceMappingsGridData'
                      class="ghd-table hide_table_scroll">                      
                      <template #bottom></template>                                  
              <template slot='items' slot-scope='props' v-slot:item="props">
                <tr>  
                    <td align="center">
                      <v-text-field
                          v-model="props.item.Attribute"                            
                          variant="underlined"
                          readonly                          
                      />
                    </td>
                    <td align="center">
                      <v-row>
                        <v-col>
                          <v-select 
                            :items="columnSelectItems"
                                  menu-icon=custom:GhdDownSvg
                                  label=""
                                  variant="outlined"
                                  item-title="text"  
                                  item-value="value"
                                  v-model="props.item.DataField"
                                  style="width: 250px;padding-top: 5px;">
                              </v-select>
                        </v-col>
                      </v-row>                        
                    </td>             
                </tr>    
              </template>
            </v-data-table-server>
          </div>          
        </div>
        <v-card-actions class="ghd-dialog-box-padding-bottom">
          <v-row justify="center">
            <CancelButton @cancel="onSubmit(false)"/>
            <SaveButton 
              @save="onSubmit(true)"
              :disabled='disableSubmitButton()'
            />
          </v-row>                    
        </v-card-actions>      
      </v-card>
    </v-dialog>
  </v-row>
</template>

<script setup lang="ts">
import Vue, { computed, ref, toRefs, shallowRef, ShallowRef, watch } from 'vue';
import {InputValidationRules, rules as validationRules} from '@/shared/utils/input-validation-rules';
import {getNewGuid} from '@/shared/utils/uuid-utils';
import { DataSourceMapping, DataSourceMappingGridData } from '../../../shared/models/iAM/data-source';
import { useStore } from 'vuex';
import { clone } from 'ramda';
import { Attribute } from '@/shared/models/iAM/attribute';
import { EditDataSourceMappingsDialogData, emptyEditDataSourceMappingsDialogData } from '@/shared/models/modals/edit-datasourcemappings-dialog-data';
import CancelButton from '@/shared/components/buttons/CancelButton.vue';
import SaveButton from '@/shared/components/buttons/SaveButton.vue';

  const stateAttributes = computed<Attribute[]>(() => store.state.attributeModule.attributes);

  let store = useStore();
  let gridHeaders: any[] = [
        { title: 'Attribute', key: 'attribute', sortable: false, align: 'center', class: '', width: '50%' },        
        { title: 'Column', key: 'dataField', sortable: false, align: 'center', class: '', width: '50%' }
    ];

  let columnSelectItems = ref<string[]>([]); // TODO assign from dataSourceExcelColumns.value.locationColumn
  const props = defineProps<{
    dialogData: EditDataSourceMappingsDialogData
  }>()
  const { dialogData } = toRefs(props);  
  const emit = defineEmits(['submit'])  
  let editDataSourceMappingsGridData = ref<DataSourceMappingGridData[]>([]);
  let rules: InputValidationRules = validationRules;
  
  watch(dialogData,() => {
      // TODO // props.dialogData.dataSourceMappings.every(_=>_.);
      // do something like  selectNetworkItems.value = stateNetworks.value.map(_ => ({text: _.name, value: _.id}));  and for Attribute find from stateAttributes
      // TODO stateAttributes wont be needed as we going to keep all attributes entries to mapping table and attribute name can be mapped
      //  using .Attribute.Name in mappig entry
      
        editDataSourceMappingsGridData.value = [{ AttributeName:'CRS', 
        DataField: props.dialogData.dataSourceMappings[0].DataField, 
        AttributeId: props.dialogData.dataSourceMappings[0].AttributeId, 
        id: props.dialogData.dataSourceMappings[0].Id,
        DataSourceId: props.dialogData.dataSourceMappings[0].DataSourceId },
        { AttributeName:'SURFACE_NAME', 
        DataField: props.dialogData.dataSourceMappings[1].DataField, 
        AttributeId: props.dialogData.dataSourceMappings[1].AttributeId, 
        id: props.dialogData.dataSourceMappings[1].Id,
        DataSourceId: props.dialogData.dataSourceMappings[1].DataSourceId }]  
    });

  function updateList(item: DataSourceMappingGridData) {
    // TODO
  }
  
  function onSubmit(submit: boolean) {
    if (submit) {
        //emit('submit', budgetChanges.value);
    } else {
        emit('submit', null);
    }

    editDataSourceMappingsGridData.value = [];
  }
  
  function disableSubmitButton() {
    // TODO?  
    return false;  
  }
  
</script>