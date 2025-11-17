import { CommonModule } from '@angular/common';
import { Component, OnInit, Output, EventEmitter } from '@angular/core';

interface Categories {
  id: string;
  name: string;
  color: string;
  emoji: string;
}

@Component({
  selector: 'app-category',
  templateUrl: './category.html',
  styleUrls: ['./category.css'],
  standalone: true,
  imports: [CommonModule]
})
export class Category implements OnInit {
  @Output() categorySelected = new EventEmitter<string>();

  categories: Categories[] = [
    { id: 'fruits', name: 'פירות', color: 'from-pink-500', emoji: '🍎' },
    { id: 'vegetables', name: 'ירקות', color: 'from-green-500', emoji: '🥕' },
    { id: 'dairy', name: 'חלב', color: 'from-blue-400', emoji: '🥛' },
    { id: 'protein', name: 'חלבונים', color: 'from-orange-500', emoji: '🥚' },
    { id: 'fish', name: 'דגים', color: 'from-teal-500', emoji: '🐟' },
    { id: 'spices', name: 'תבלינים', color: 'from-red-500', emoji: '🌶️' },
    { id: 'herbs', name: 'עשבי תיבול', color: 'from-lime-500', emoji: '🌿' },
    { id: 'baking', name: 'אפייה', color: 'from-yellow-500', emoji: '🍪' }
  ];

  constructor() { }

  ngOnInit(): void { }

  selectCategory(categoryId: string): void {
    this.categorySelected.emit(categoryId);
  }
}