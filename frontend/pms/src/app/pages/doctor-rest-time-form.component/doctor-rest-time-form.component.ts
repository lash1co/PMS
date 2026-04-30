import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RestTimeService } from '../../services/rest-time/rest-time.service';
import { DoctorRestTimeModal } from '../doctor-rest-time-modal/doctor-rest-time-modal';

@Component({
  selector: 'app-doctor-rest-time-form',
  imports: [CommonModule, FormsModule, DoctorRestTimeModal],
  templateUrl: './doctor-rest-time-form.component.html',
  styleUrl: './doctor-rest-time-form.component.css',
})
export class DoctorRestTimeFormComponent implements OnInit {
  restingTimeData = signal<RestTimeInterface[]>([]);

  currentRestTime: RestTimeInterface = {
    id: 0,
    startTime: "00:00",
    endTime: "00:00",
    reason: ''
  };

  showModal = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  constructor(private restTimeService: RestTimeService) {}

  ngOnInit(): void {
    this.loadRestTimes();
  }

  loadRestTimes(): void {
    this.restTimeService.getRestTimes().subscribe({
      next: (restTimes) => {
        this.restingTimeData.set(restTimes);
      },
      error: (error) => {
        console.error('Error loading rest times:', error);
        alert('Failed to load rest times. Please try again.');
      }
    });
  }

  openCreateModal(): void {
    this.isEditing.set(false);
    this.currentRestTime = {
      id: 0,
      startTime: "00:00",
      endTime: "00:00",
      reason: ''
    };
    this.showModal.set(true);
  }

  openEditModal(restTime: RestTimeInterface): void {
    this.isEditing.set(true);
    this.currentRestTime = { ...restTime };
    this.showModal.set(true);
  }

  saveRestTime(restTime: RestTimeInterface): void {
    if (this.isEditing()) {
      // Handle update
      this.restTimeService.updateRestTime(restTime).subscribe({
        next: (response) => {
          alert(`Rest time updated successfully!`);
          this.closeModal();
          this.loadRestTimes();
        },
        error: (error) => {
          console.error('Error updating rest time:', error);
          alert('Failed to update rest time. Please try again.');
        }
      });
    } else {
      // Handle create
      this.restTimeService.createRestTime(restTime).subscribe({
        next: (response) => {
          alert(`Rest time created successfully!`);
          this.closeModal();
          this.loadRestTimes();
        },
        error: (error) => {
          console.error('Error creating rest time:', error);
          alert('Failed to create rest time. Please try again.');
        }
      });
    }
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  deleteRestTime(id?: number): void {
    if (id && confirm('Are you sure to remove this rest time? This action cannot be undone.')) {
      this.restTimeService.deleteRestTime(id).subscribe({
        next: () => {
          alert('Rest time deleted successfully!');
          this.loadRestTimes();
        },
        error: (error) => {
          console.error('Error deleting rest time:', error);
          alert('Failed to delete rest time. Please try again.');
        }
      });
    }
  }
}
