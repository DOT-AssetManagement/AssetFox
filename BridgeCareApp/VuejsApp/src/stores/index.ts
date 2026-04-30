import { createPinia } from 'pinia';

const pinia = createPinia();

export * from './alert.store';
export * from './busy.store';
export * from './unsaved-changes.store';
export default pinia;
