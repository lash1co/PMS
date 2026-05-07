import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrescriptionService } from '../../services/prescription/prescription.service';
import { Prescription } from '../../models/prescription.model';
import { PrescriptionModalComponent } from '../prescription-modal/prescription-modal.component';

@Component({
  selector: 'app-prescriptions-list',
  standalone: true,
  imports: [CommonModule, PrescriptionModalComponent],
  templateUrl: './prescriptions-list.html'
})
export class PrescriptionsList implements OnInit {

  prescriptions = signal<Prescription[]>([]);
  showModal = signal(false);
  selectedPrescription = signal<Prescription | null>(null);

  constructor(private service: PrescriptionService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.service.getAll().subscribe(data => {
      this.prescriptions.set(data || []);
    });
  }

  openCreateModal() {
    this.selectedPrescription.set(null);
    this.showModal.set(true);
  }

  openEditModal(p: Prescription) {
    this.selectedPrescription.set(p);
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  delete(id: number) {
    if (!confirm('Delete this prescription?')) return;

    this.service.delete(id).subscribe(() => this.load());
  }
}