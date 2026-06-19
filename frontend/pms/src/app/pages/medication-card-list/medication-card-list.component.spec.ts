import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MedicationCardListComponent } from './medication-card-list.component';

describe('MedicationCardListComponent', () => {
  let component: MedicationCardListComponent;
  let fixture: ComponentFixture<MedicationCardListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MedicationCardListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MedicationCardListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
