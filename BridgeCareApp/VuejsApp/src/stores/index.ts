import { createPinia } from 'pinia';

const pinia = createPinia();

export * from './alert.store';
export * from './admin-site-settings.store';
export * from './admin-data.store';
export * from './analysis-method.store';
export * from './announcement.store';
export * from './authentication.store';
export * from './azure-b2c.store';
export * from './busy.store';
export * from './config.store';
export * from './notification.store';
export * from './unsaved-changes.store';
export * from './user.store';
export default pinia;
