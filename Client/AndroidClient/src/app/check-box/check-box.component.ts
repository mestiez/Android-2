import { Component, EventEmitter, HostListener, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-check-box',
  templateUrl: './check-box.component.html',
  styleUrls: ['./check-box.component.less']
})
export class CheckBoxComponent implements OnInit {

  @Input() checked = false;
  @Output() checkChange = new EventEmitter<boolean>();

  constructor() { }

  ngOnInit(): void {

  }

  @HostListener('click') onClick(): void {
    this.checked = !this.checked;
    this.checkChange.emit(this.checked);
  }
}
