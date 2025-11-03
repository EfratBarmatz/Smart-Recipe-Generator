import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-product-card',
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
  standalone: true
})
export class ProductCard {
  @Input() product: any;
  @Output() toggleSelect = new EventEmitter<any>();

  selected = false;

  onClick() {
    this.selected = !this.selected;
    this.toggleSelect.emit({ ...this.product, selected: this.selected });
  }
}
