import { createPinia } from 'pinia';

const pinia = createPinia();

export * from './alert.store';
export * from './authentication.store';
export * from './busy.store';
export * from './config.store';
export * from './notification.store';
export * from './unsaved-changes.store';
export * from './user.store';
export default pinia;
