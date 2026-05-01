import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EncounterModal } from './encounter-modal';

describe('EncounterModal', () => {
  let component: EncounterModal;
  let fixture: ComponentFixture<EncounterModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EncounterModal],
    }).compileComponents();

    fixture = TestBed.createComponent(EncounterModal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
