import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, isNil, propEq, update } from 'ramda';
import AttributeService from '@/services/attribute.service';
import {
    Attribute,
    AttributeSelectValues,
    AttributeSelectValuesResult,
    emptyAttribute,
    RuleDefinition,
} from '@/shared/models/iAM/attribute';
import { noneDatasource } from '@/shared/models/iAM/data-source';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { useNotificationStore } from './notification.store';

export interface AttributeStoreState {
    attributes: Attribute[];
    selectedAttribute: Attribute;
    stringAttributes: Attribute[];
    numericAttributes: Attribute[];
    attributesSelectValues: AttributeSelectValues[];
    checkedSelectAttributes: string[];
    attributeDataSourceTypes: string[];
    aggregationRules: RuleDefinition[];
    aggregationRulesForType: string[];
}

function sanitizeAttributes(attributes: Attribute[]): Attribute[] {
    return attributes.map((attribute: Attribute) => {
        if (isNil(attribute.dataSource)) {
            return {
                ...attribute,
                dataSource: clone(noneDatasource),
            };
        }

        return attribute;
    });
}

function sortAttributes(attributes: Attribute[]): Attribute[] {
    return [...attributes].sort((first: Attribute, second: Attribute) =>
        first.name < second.name ? -1 : 1,
    );
}

function selectAttributeById(
    attributes: Attribute[],
    attributeId: string,
): Attribute {
    const selectedAttribute = find(
        propEq('id', attributeId),
        attributes,
    ) as Attribute | undefined;

    return selectedAttribute ? selectedAttribute : clone(emptyAttribute);
}

function upsertAttributeState(
    attributes: Attribute[],
    attribute: Attribute,
): Attribute[] {
    const nextAttributes = any(propEq('id', attribute.id), attributes)
        ? (update(
              findIndex(propEq('id', attribute.id), attributes),
              attribute,
              attributes,
          ) as Attribute[])
        : (append(attribute, attributes) as Attribute[]);

    return sortAttributes(nextAttributes);
}

function filterAttributesByType(attributes: Attribute[], type: string): Attribute[] {
    return attributes.filter((attribute: Attribute) => attribute.type === type);
}

function applyClonedAttributesState(
    state: Pick<AttributeStoreState, 'attributes'>,
    attributes: Attribute[],
) {
    const sanitizedAttributes = sanitizeAttributes(attributes);
    state.attributes = clone(sanitizedAttributes);
}

function applyFetchedAttributesState(
    state: Pick<
        AttributeStoreState,
        'attributes' | 'stringAttributes' | 'numericAttributes'
    >,
    attributes: Attribute[],
) {
    applyClonedAttributesState(state, attributes);
    const sanitizedAttributes = state.attributes;
    state.stringAttributes = filterAttributesByType(sanitizedAttributes, 'STRING');
    state.numericAttributes = filterAttributesByType(sanitizedAttributes, 'NUMBER');
}

function appendAttributeSelectValues(
    existingValues: AttributeSelectValues[],
    nextValues: AttributeSelectValues[],
): AttributeSelectValues[] {
    return [...existingValues, ...nextValues];
}

function appendCheckedSelectAttributes(
    existingAttributes: string[],
    nextAttributes: string[],
): string[] {
    return [...existingAttributes, ...nextAttributes];
}

function getAggregationRuleNames(
    aggregationRules: RuleDefinition[],
    type: string,
): string[] {
    const filteredRules = aggregationRules.filter((ruleDefinition: RuleDefinition) =>
        type === 'NUMBER' ? ruleDefinition.isNumeric : ruleDefinition.isText,
    );

    return clone(
        filteredRules.map(
            (aggregationRule: RuleDefinition) => aggregationRule.ruleName,
        ),
    );
}

export const useAttributeStore = defineStore('attribute', {
    state: (): AttributeStoreState => ({
        attributes: [],
        selectedAttribute: clone(emptyAttribute),
        stringAttributes: [],
        numericAttributes: [],
        attributesSelectValues: [],
        checkedSelectAttributes: [],
        attributeDataSourceTypes: [],
        aggregationRules: [],
        aggregationRulesForType: [],
    }),
    actions: {
        selectAttribute(attributeId: string) {
            this.selectedAttribute = selectAttributeById(
                this.attributes,
                attributeId,
            );
        },
        async getAttributes() {
            await AttributeService.getAttributes().then(
                (response: AxiosResponse<Attribute[]>) => {
                    if (hasValue(response, 'data')) {
                        applyFetchedAttributesState(this, response.data);
                    }
                },
            );
        },
        async getAttributeSelectValues(attributeNames: string[]) {
            const notificationStore = useNotificationStore();
            await AttributeService.getAttributeSelectValues(attributeNames).then(
                (response: AxiosResponse<AttributeSelectValuesResult[]>) => {
                    if (hasValue(response, 'data')) {
                        const attributesSelectValues: AttributeSelectValues[] = [];
                        const warningMessages: string[] = [];

                        response.data.forEach((result: AttributeSelectValuesResult) => {
                            attributesSelectValues.push({
                                attribute: result.attribute,
                                values: result.values,
                            });

                            if (result.resultType.toLowerCase() === 'warning') {
                                warningMessages.push(result.resultMessage);
                            }
                        });

                        this.attributesSelectValues = appendAttributeSelectValues(
                            this.attributesSelectValues,
                            attributesSelectValues,
                        );
                        this.checkedSelectAttributes = appendCheckedSelectAttributes(
                            this.checkedSelectAttributes,
                            attributeNames,
                        );

                        if (hasValue(warningMessages)) {
                            notificationStore.addWarningNotification({
                                message: 'Attributes selected warning.',
                                longMessage:
                                    warningMessages.length === 1
                                        ? warningMessages[0]
                                        : warningMessages.join('<br>'),
                            });
                        }
                    }
                },
            );
        },
        async upsertAttributes(attributes: Attribute[]) {
            const notificationStore = useNotificationStore();
            await AttributeService.upsertAttributes(attributes).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        applyClonedAttributesState(this, attributes);
                        notificationStore.addSuccessNotification({
                            message: 'Modified attributes',
                        });
                    }
                },
            );
        },
        async upsertAttribute(attribute: Attribute) {
            const notificationStore = useNotificationStore();
            await AttributeService.upsertAttribute(attribute).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        const message = any(propEq('id', attribute.id), this.attributes)
                            ? 'Updated attribute'
                            : 'Added attribute';

                        this.attributes = upsertAttributeState(
                            this.attributes,
                            attribute,
                        );
                        this.stringAttributes = filterAttributesByType(
                            this.attributes,
                            'STRING',
                        );
                        this.numericAttributes = filterAttributesByType(
                            this.attributes,
                            'NUMBER',
                        );
                        this.selectedAttribute = selectAttributeById(
                            this.attributes,
                            attribute.id,
                        );
                        notificationStore.addSuccessNotification({ message });
                    }
                },
            );
        },
        async getAttributeAggregationRules() {
            await AttributeService.GetAttributeAggregationRules().then(
                (response: AxiosResponse<RuleDefinition[]>) => {
                    if (hasValue(response, 'data')) {
                        this.aggregationRules = clone(response.data);
                    }
                },
            );
        },
        getAggregationRulesForType(type: string) {
            if (type === 'NUMBER') {
                this.aggregationRulesForType = getAggregationRuleNames(
                    this.aggregationRules,
                    type,
                );
            }

            if (type === 'STRING') {
                this.aggregationRulesForType = getAggregationRuleNames(
                    this.aggregationRules,
                    type,
                );
            }
        },
        async getAttributeDataSourceTypes() {
            await AttributeService.GetAttributeDataSourceTypes().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.attributeDataSourceTypes = clone(response.data);
                    }
                },
            );
        },
    },
});