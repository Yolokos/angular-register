import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from './user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  readonly rootUrl = "https://localhost:5001";

  constructor(private http: HttpClient) { }

  registerUser(user: User){
      const body: User = {
        UserName: user.UserName,
        Email: user.Email,
        Password: user.Password,
        FirstName: user.FirstName,
        LastName: user.LastName
      }

      return this.http.post(this.rootUrl + "/register", body);
  }
}
