import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  messages = ' my name is Shivender';
  users: any = [];

  constructor(private http: HttpClient) {}
  ngOnInit() {}

  getUsers() {
    this.http.get('https://localhost:5001/api/Users').subscribe(
      (Response) => {
        setTimeout(() => {
          this.users = Response;
          this.messages = '';
        }, 2000);
      },
      (error) => {
        console.log(error);
      }
    );
  }

  click = (messages: string) => {
    // alert('hiii' + messages);
    this.messages = 'Fetching Data';
    this.getUsers();
  };
}
