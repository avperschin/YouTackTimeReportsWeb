import * as ng from '@angular/core';
import { Http, URLSearchParams } from '@angular/http';
import {FORM_DIRECTIVES} from '@angular/common';
import {LocalStorage} from 'ng2-webstorage';
import {DatepickerComponent} from "../common/datepicker.component";
import * as moment from 'moment';

@ng.Pipe({ name: 'unixToDate' })
export class UnixToDatePipe implements ng.PipeTransform {
    transform(value: Date, args: string[]): any {
        return moment(value).locale('ru').format(args[0] || 'ddd');
    }
}

@ng.Component({
    selector: 'report',
    template: require('./report.html'),
    directives: [FORM_DIRECTIVES, DatepickerComponent],
    pipes: [UnixToDatePipe]
})
export class Report {
    @LocalStorage()
    public settingsFill;
    @LocalStorage()
    public host;
    @LocalStorage()
    public port;
    @LocalStorage()
    public path;
    @LocalStorage()
    public login;
    @LocalStorage()
    public password;
    @LocalStorage()
    public isssl;
    public dateStart: any; 
    public dateEnd: any;
    public projectId: string;
    public message: string = 'Заполните требуемые параметры!';
    public errorMessage: string;
    public status: boolean;
    public loading: boolean;
    public report: IResultsData;

    constructor(private http: Http) { }

    sendForReport() {
        this.report = null;
        this.loading = true;

        let reportUrl = '/api/report';
        let params = new URLSearchParams();
        params.set('host', this.host);
        params.set('port', this.port);
        params.set('path', this.path);
        params.set('login', this.login);
        params.set('password', this.password);
        params.set('isssl', this.isssl);
        params.set('datestart', this.dateStart.format('DD.MM.YYYY'));
        params.set('dateend', this.dateEnd.format('DD.MM.YYYY'));
        params.set('projectid', this.projectId);

        this.http.get(reportUrl, { search: params }).subscribe(result => {
            if (result.json().status === 0) {
                this.status = true;
                this.errorMessage = result.json().error;
            }
            if (result.json().status === 1) {
                this.status = false;
                this.errorMessage = null;
                this.report = result.json().report;
            }
            this.loading = false;
        });
    }
}

interface IResultsData {
    userReports: IUserReport[];
    dateList: IDateList[];
    duration: number;
    estimation: number;
    norm: number;
    durationToday: number;
    estimationToday: number;
    normToday: number;
    projectName: string;
}

interface IUserReport {
    userName: string;
    userLogin: string;
    groupName: string;
    workingDays: IWorkingDays[];
    duration: number;
    dstimation: number;
    norm: number;
}

interface IWorkingDays {
    dateTime: Date;
    duration: number;
    estimation: number;
    norm: number;
    weekNumber: number;
    isWorkingDay: boolean;
}

interface IDateList {
    date: Date;
    duration: number;
    estimation: number;
    weekNumber: number;
    isMaxDateOfWeek: boolean;
    durationWeek: number;
    estimationWeek: number;
}