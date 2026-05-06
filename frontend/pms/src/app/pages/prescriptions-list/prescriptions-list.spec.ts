import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrescriptionsList } from './prescriptions-list';

describe('PrescriptionsList', () => {
  let component: PrescriptionsList;
  let fixture: ComponentFixture<PrescriptionsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrescriptionsList],
    }).compileComponents();

    fixture = TestBed.createComponent(PrescriptionsList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
