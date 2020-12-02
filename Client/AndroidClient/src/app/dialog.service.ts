import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DialogService {

  public currentDialog: {
    question: string,
    action: () => void
  } = null;

  constructor() { }
}
