import { Component } from '@angular/core';
import { PageTitleService } from '../../shared/services/page-title-service';

@Component({
  selector: 'app-startup',
  imports: [],
  templateUrl: './startup.html',
  styleUrl: './startup.css'
})
export class Startup {

  constructor(private pageTitleService: PageTitleService) {};

  ngOnInit() {
    this.pageTitleService.setTitle('Home');
  }
}
