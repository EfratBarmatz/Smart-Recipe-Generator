import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { Preferences } from '../../models/recipe-request';
import { PreferencesComponent } from '../preferences/preferences';

interface Product {
  name: string;
  selected: boolean;
}

@Component({
  selector: 'app-product-card',
  templateUrl: './product-card.html',
  styleUrls: ['./product-card.css'],
  standalone: true,
 imports: [CommonModule, PreferencesComponent]

})
export class ProductCard implements OnChanges {
  @Input() categoryId: string = '';
  @Output() backToCategories = new EventEmitter<void>();
  @Output() generateRecipe = new EventEmitter<{ products: string[], servings: number }>();

  servings: number = 2;

  // שמירת כל המוצרים שנבחרו מכל הקטגוריות
  private allSelectedProducts: string[] = [];

  products: { [key: string]: Product[] } = {
    fruits: [
      { name: 'תפוח', selected: false },
      { name: 'בננה', selected: false },
      { name: 'תפוז', selected: false },
      { name: 'תות', selected: false },
      { name: 'מנגו', selected: false },
      { name: 'אבוקדו', selected: false }
    ],
    vegetables: [
      { name: 'עגבנייה', selected: false },
      { name: 'מלפפון', selected: false },
      { name: 'גזר', selected: false },
      { name: 'בצל', selected: false },
      { name: 'שום', selected: false },
      { name: 'פלפל', selected: false }
    ],
    dairy: [
      { name: 'חלב', selected: false },
      { name: 'גבינה צהובה', selected: false },
      { name: 'יוגורט', selected: false },
      { name: 'שמנת', selected: false },
      { name: 'חמאה', selected: false },
      { name: 'גבינת קוטג׳', selected: false }
    ],
    protein: [
      { name: 'ביצים', selected: false },
      { name: 'עוף', selected: false },
      { name: 'בשר בקר', selected: false },
      { name: 'חזה הודו', selected: false },
      { name: 'טופו', selected: false }
    ],
    fish: [
      { name: 'סלמון', selected: false },
      { name: 'טונה', selected: false },
      { name: 'בקלה', selected: false },
      { name: 'דניס', selected: false },
      { name: 'לברק', selected: false }
    ],
    spices: [
      { name: 'כורכום', selected: false },
      { name: 'כמון', selected: false },
      { name: 'פפריקה', selected: false },
      { name: 'קארי', selected: false },
      { name: 'פלפל שחור', selected: false },
      { name: 'קינמון', selected: false }
    ],
    herbs: [
      { name: 'פטרוזיליה', selected: false },
      { name: 'כוסברה', selected: false },
      { name: 'בזיליקום', selected: false },
      { name: 'נענע', selected: false },
      { name: 'רוזמרין', selected: false },
      { name: 'זעתר', selected: false }
    ],
    baking: [
      { name: 'קמח', selected: false },
      { name: 'סוכר', selected: false },
      { name: 'שמרים', selected: false },
      { name: 'אבקת אפייה', selected: false },
      { name: 'וניל', selected: false },
      { name: 'שוקולד', selected: false }
    ]
  };

  ngOnChanges(): void {
    // כשעוברים לקטגוריה חדשה, שמור את המוצרים שנבחרו בקטגוריה הקודמת
    this.saveCurrentCategorySelections();
    // שחזר את הבחירות של הקטגוריה הנוכחית
    this.restoreCurrentCategorySelections();
  }

  private saveCurrentCategorySelections(): void {
    // הסר מוצרים מהקטגוריה הנוכחית מהרשימה הכללית
    const currentProducts = this.currentProducts.map(p => p.name);
    this.allSelectedProducts = this.allSelectedProducts.filter(
      product => !currentProducts.includes(product)
    );

    // הוסף מוצרים נבחרים מהקטגוריה הנוכחית
    const selectedInCategory = this.currentProducts
      .filter(p => p.selected)
      .map(p => p.name);

    this.allSelectedProducts.push(...selectedInCategory);
  }

  private restoreCurrentCategorySelections(): void {
    // שחזר את הבחירות של הקטגוריה הנוכחית
    this.currentProducts.forEach(product => {
      product.selected = this.allSelectedProducts.includes(product.name);
    });
  }

  get currentProducts(): Product[] {
    return this.products[this.categoryId] || [];
  }

  get selectedProducts(): string[] {
    // החזר את כל המוצרים שנבחרו מכל הקטגוריות
    return this.allSelectedProducts;
  }

  get hasSelectedProducts(): boolean {
    return this.allSelectedProducts.length > 0;
  }

  toggleProduct(product: Product): void {
    product.selected = !product.selected;

    if (product.selected) {
      // הוסף מוצר לרשימה הכללית
      if (!this.allSelectedProducts.includes(product.name)) {
        this.allSelectedProducts.push(product.name);
      }
    } else {
      // הסר מוצר מהרשימה הכללית
      this.allSelectedProducts = this.allSelectedProducts.filter(
        p => p !== product.name
      );
    }
  }

  increaseServings(): void {
    this.servings++;
  }

  decreaseServings(): void {
    if (this.servings > 1) {
      this.servings--;
    }
  }

  goBack(): void {
    // שמור את הבחירות לפני חזרה
    this.saveCurrentCategorySelections();
    this.backToCategories.emit();
  }

  onGenerateRecipe(): void {
    // שמור את הבחירות לפני יצירת המתכון
    this.saveCurrentCategorySelections();

    this.generateRecipe.emit({
      products: this.allSelectedProducts,
      servings: this.servings
    });
  }

  @Output() preferencesUpdated = new EventEmitter<Preferences>();
  preferences: Preferences = { vegetarian: false, vegan: false, glutenFree: false };

  onPreferencesChange(prefs: Preferences): void {
    this.preferences = prefs;
    this.preferencesUpdated.emit(prefs);
  }
}