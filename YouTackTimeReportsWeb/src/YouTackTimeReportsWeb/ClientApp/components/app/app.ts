import * as ng from '@angular/core';
import { Router, ROUTER_DIRECTIVES } from '@angular/router';
import { Http } from '@angular/http';
import { NavMenu } from '../nav-menu/nav-menu';
import { LocalStorage } from 'ng2-webstorage';

@ng.Component({
    selector: 'app',
    template: require('./app.html'),
    directives: [...ROUTER_DIRECTIVES, NavMenu]
})
export class App {
    @LocalStorage()
    private settingsFill: boolean;
    constructor(private router: Router) {
        if (!this.settingsFill) {
            this.router.navigate(['./settings']);
        }
    }
}
