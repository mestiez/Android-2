import { Component, OnInit } from '@angular/core';
import { DialogService } from '../dialog.service';



@Component({
  selector: 'app-dialog-screen',
  templateUrl: './dialog-screen.component.html',
  styleUrls: ['./dialog-screen.component.less']
})
export class DialogScreenComponent implements OnInit {

  constructor(public dialogService: DialogService) { }

  ngOnInit(): void {
  }

  public closeDialog() {
    this.dialogService.currentDialog = null;
  }
}
