<template>
  <v-row>
    <v-dialog style='max-height: 750px; max-width: 900px;' persistent scrollable v-model ='dialogData.showDialog'>
      <v-card>
        <v-card-title class="ghd-dialog-box-padding-top">
          <v-row justify-space-between align-center>
            <v-col>
              <div class="ghd-control-dialog-header">Edit Data Source Mappings</div>
              <v-spacer></v-spacer>
              <XButton @click="onSubmit(false)"/>
              </v-col>
              <v-col>
                <v-btn @click='downloadDataSourceMappings' 
                  class="ghd-blue ghd-button-text ghd-outline-button-padding ghd-button" style="margin: 5px;float: right;" variant="outlined">Download</v-btn>
                </v-col>
          </v-row>
        </v-card-title>
        <div style='max-height: 700px; max-width:900px; margin-top:10px;' class="ghd-dialog-box-padding-center">
          <div style='max-height: 550px; overflow-y:auto;'>
            <v-data-table-server
                      id="EditDataSourceMappingsDialog-dataTable"
                      :headers='gridHeaders'
                      :items="editDataSourceMappingsData"
                      :items-length="editDataSourceMappingsData.length"
                      sort-asc-icon="custom:GhdTableSortAscSvg"
                      sort-desc-icon="custom:GhdTableSortDescSvg"
                      hide-actions
                      item-key='id'
                      v-model='editDataSourceMappingsData'
                      class="ghd-table hide_table_scroll"
                      style='max-height: 550px;'>
                      <template #bottom></template>                                  
              <template slot='items' slot-scope='props' v-slot:item="props">
                <tr>  
                    <td align="center" style="height: 40px;">
                      <v-text-field
                          v-model="props.item.attributeName"                            
                          variant="underlined"                          
                          readonly                          
                      />
                    </td>
                    <td align="center" style="height: 40px;">                      
                      <v-select 
                        :items="columnSelectItems"
                              menu-icon=custom:GhdDownSvg
                              label=""
                              variant="outlined"
                              item-title="text"  
                              item-value="value"
                              v-model="props.item.dataField"
                              style="width: 250px; padding-top: 5px;">
                          </v-select>
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
import Vue, { computed, ref, toRefs, watch } from 'vue';
import { DataSourceMappingData } from '../../../shared/models/iAM/data-source';
import { useStore } from 'vuex';
import { EditDataSourceMappingsDialogData } from '@/shared/models/modals/edit-datasourcemappings-dialog-data';
import CancelButton from '@/shared/components/buttons/CancelButton.vue';
import SaveButton from '@/shared/components/buttons/SaveButton.vue';
import DataSourceService from '@/services/data-source.service';
import { AxiosResponse } from 'axios';
import { hasValue } from '@/shared/utils/has-value-util';
import { FileInfo } from '@/shared/models/iAM/file-info';
import FileDownload from 'js-file-download';
import { convertBase64ToArrayBuffer } from '@/shared/utils/file-utils';

  let store = useStore();
  let gridHeaders: any[] = [
        { title: 'Attribute', key: 'attribute', sortable: false, align: 'center', class: '', width: '50%' },        
        { title: 'Column', key: 'dataField', sortable: false, align: 'center', class: '', width: '50%' }
    ];
  
  const props = defineProps<{
    dialogData: EditDataSourceMappingsDialogData
  }>()
  const { dialogData } = toRefs(props);  
  const emit = defineEmits(['submit'])  
  let editDataSourceMappingsData = ref<DataSourceMappingData[]>([]);
  let columnSelectItems = ref<string[]>([]);
  let dataSourceId = ref<string>();
  
  watch(dialogData,() => {      
      columnSelectItems.value = dialogData.value.columnSelectItems;
      editDataSourceMappingsData.value = dialogData.value.dataSourceMappings;
      dataSourceId.value = dialogData.value.dataSourceId;
  });
  
  function onSubmit(submit: boolean) {
    if (submit) {
        emit('submit', editDataSourceMappingsData.value);
    } else {
        emit('submit', null);
    }

    editDataSourceMappingsData.value = [];
  }
  
  function disableSubmitButton() {
    // TODO?  
    return false;  
  }

  function downloadDataSourceMappings(){
  DataSourceService.downloadDataSourceMappings(dialogData.value.dataSourceId)
      .then((response: AxiosResponse) => {
          if (hasValue(response, 'data')) {
              const fileInfo: FileInfo = response.data as FileInfo;
              FileDownload(convertBase64ToArrayBuffer(fileInfo.fileData), fileInfo.fileName, fileInfo.mimeType);
          }
      });
  }
  
</script>