import * as ng from '@angular/core';
declare var $: any;
@ng.Component({
    selector: 'ng2-datepicker',
    template: require('./datepicker.component.html')
})
export class DatepickerComponent implements ng.AfterViewChecked {
    @ng.Input() ID: string;
    @ng.Output() dateSelected: ng.EventEmitter<string> = new ng.EventEmitter<string>(false);
    private date: string;
    ngAfterViewChecked(): void {
        let datepickerElement = $('#' + this.ID);
        datepickerElement.datetimepicker();
        datepickerElement.on('dp.change', e => {
            this.date = e.date;
            this.dateSelected.emit(this.date);
        });
    }
}