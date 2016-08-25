import * as ng from '@angular/core';
import {LocalStorageService} from 'ng2-webstorage';

@ng.Component({
    selector: 'settings',
    template: require('./settings.html')
})
export class Settings {
    public settingsFill: boolean;
    public host: string;
    public port: number;
    public path: string;
    public login: string;
    public password: string;
    public isssl: boolean;
    constructor(private storage: LocalStorageService) {
        this.host = this.storage.retrieve('host');
        this.port = this.storage.retrieve('port');
        this.path = this.storage.retrieve('path');
        this.login = this.storage.retrieve('login');
        this.isssl = this.storage.retrieve('isssl');
        this.password = this.storage.retrieve('password');
        this.settingsFill = this.storage.retrieve('settingsFill');
    }
    saveSettings() {
        this.settingsFill = true;
        this.storage.store('host', this.host);
        this.storage.store('port', this.port);
        this.storage.store('path', this.path);
        this.storage.store('login', this.login);
        this.storage.store('password', this.password);
        this.storage.store('isssl', this.isssl);
        this.storage.store('settingsFill', this.settingsFill);
    }
}