import { Component, Input } from '@angular/core';
import { RecipeResponse } from '../../models/recipe-response';

@Component({
  selector: 'app-quickly-recipe',
  imports: [],
  templateUrl: './quickly-recipe.html',
  styleUrl: './quickly-recipe.css',
  standalone: true
})
export class QuicklyRecipe {
  @Input() recipe: RecipeResponse | null = null;
}