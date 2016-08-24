import { RouterConfig } from '@angular/router';
import { Report } from './components/report/report';
import { Settings } from './components/settings/settings';

export const routes: RouterConfig = [
    { path: '', component: Report },
    { path: 'settings', component: Settings }
];
