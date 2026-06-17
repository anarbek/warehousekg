import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  roles: string[];
}

export interface CreateUserDto {
  userName: string;
  email: string;
  password: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly base = `${AppSettings.apiBaseUrl}/users`;

  getUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.base);
  }

  createUser(dto: CreateUserDto): Observable<string> {
    return this.http.post<string>(this.base, dto);
  }

  updateRoles(userId: string, roles: string[]): Observable<void> {
    return this.http.put<void>(`${this.base}/${userId}/roles`, roles);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getAvailableRoles(): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/roles`);
  }
}
