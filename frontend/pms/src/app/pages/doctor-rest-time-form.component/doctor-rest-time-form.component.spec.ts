import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DoctorRestTimeFormComponent } from './doctor-rest-time-form.component';

describe('DoctorRestTimeFormComponent', () => {
  let component: DoctorRestTimeFormComponent;
  let fixture: ComponentFixture<DoctorRestTimeFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorRestTimeFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorRestTimeFormComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
