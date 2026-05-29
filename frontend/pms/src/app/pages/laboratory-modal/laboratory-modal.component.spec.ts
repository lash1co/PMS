import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LaboratoryModalComponent } from './laboratory-modal.component';

describe('LaboratoryModalComponent', () => {
  let component: LaboratoryModalComponent;
  let fixture: ComponentFixture<LaboratoryModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LaboratoryModalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(LaboratoryModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
