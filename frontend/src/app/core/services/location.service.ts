import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LocationItem } from '../models/location.models';

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  public getLocations(): Observable<LocationItem[]> {
    return this.http.get<LocationItem[]>(`${this.apiUrl}/locations`);
  }
}

