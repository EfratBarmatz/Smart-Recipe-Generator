import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuicklyRecipe } from './quickly-recipe';

describe('QuicklyRecipe', () => {
  let component: QuicklyRecipe;
  let fixture: ComponentFixture<QuicklyRecipe>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuicklyRecipe]
    })
    .compileComponents();

    fixture = TestBed.createComponent(QuicklyRecipe);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
