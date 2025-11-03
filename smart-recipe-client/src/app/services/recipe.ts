import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RecipeRequest } from '../models/recipe-request';
import { RecipeResponse } from '../models/recipe-response';

@Injectable({
  providedIn: 'root'
})
export class RecipeService {
  private apiUrl = 'http://localhost:5002/api/Recipes';

  constructor(private http: HttpClient) { }

  generateRecipe(request: RecipeRequest): Observable<RecipeResponse> {
    return this.http.post<RecipeResponse>(`${this.apiUrl}/generate`, request);
  }
}