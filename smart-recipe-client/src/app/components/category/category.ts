import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-category',
  templateUrl: './category.html',
  styleUrl: './category.css',
  standalone: true,
  imports: [CommonModule]
})
export class Category {
  categories = ['Fruits', 'Vegetables', 'Dairy', 'Spices'];
  selectedCategory: string = '';

  @Output() categorySelected = new EventEmitter<string>();

  selectCategory(category: string) {
    this.selectedCategory = category;
    this.categorySelected.emit(category);
  }
}
