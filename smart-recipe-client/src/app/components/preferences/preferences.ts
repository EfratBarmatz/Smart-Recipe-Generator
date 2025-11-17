import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Preferences } from '../../models/recipe-request';

@Component({
  selector: 'app-preferences',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './preferences.html',
  styleUrls: ['./preferences.css']
})
export class PreferencesComponent {
  @Input() preferences: Preferences = {
    vegetarian: false,
    vegan: false,
    glutenFree: false,
  };
  @Output() preferencesChange = new EventEmitter<Preferences>();

  updatePreference(key: keyof Preferences, value: any) {
    this.preferences = { ...this.preferences, [key]: value };
    this.preferencesChange.emit(this.preferences);
  }
}
