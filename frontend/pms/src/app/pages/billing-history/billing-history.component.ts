import { Component, inject, signal, computed, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BillingService } from '../../services/billing/billing.service';
import { PatientService } from '../../services/patient/patient.service';
import { InvoiceHistoryResult } from '../../Entities/Billing/billing-history';
import { BillingHistoryModalComponent } from './billing-history-modal/billing-history-modal.component';

@Component({
  selector: 'app-billing-history',
  standalone: true,
  imports: [CommonModule, FormsModule, BillingHistoryModalComponent],
  templateUrl: './billing-history.component.html',
})
export class BillingHistoryComponent {
  private billingService = inject(BillingService);
  private patientService = inject(PatientService);

  // Filters
  startDate = signal<string>('');
  endDate = signal<string>('');
  includeCompleted = signal<boolean>(false);

  // Real Database Lists
  dbPatients = signal<{id: number, name: string}[]>([]);
  selectedPatientsList = signal<{id: number, name: string}[]>([]);
  selectedPatients = computed(() => this.selectedPatientsList().map(p => p.id));
  patientSearch = signal<string>('');

  // Results State
  historyResults = signal<InvoiceHistoryResult[]>([]);
  isLoading = signal<boolean>(false);
  isSearchEnabled = computed(() => !!this.startDate() && !!this.endDate());

  // Modal State
  selectedInvoiceId = signal<number | null>(null);

  constructor() {
    afterNextRender(() => {
      this.loadCatalogData();
    });
  }

  private loadCatalogData() {
    this.patientService.getPatients().subscribe({
      next: (patients) => {
        const mappedPatients = patients.map(p => ({
          id: p.id!,
          name: `${p.lastName} ${p.firstName}`
        }));
        this.dbPatients.set(mappedPatients);
      },
      error: (err) => console.error('Error loading patients catalog', err)
    });
  }

  filteredPatients = computed(() => {
    const term = this.patientSearch().toLowerCase();
    if (!term) return [];
    return this.dbPatients().filter(p => 
      p.name.toLowerCase().includes(term) && !this.selectedPatients().includes(p.id)
    );
  });

  addPatient(patient: {id: number, name: string}) {
    this.selectedPatientsList.update(list => [...list, patient]);
    this.patientSearch.set('');
  }

  removePatient(id: number) {
    this.selectedPatientsList.update(list => list.filter(p => p.id !== id));
  }

  searchHistory() {
    if (!this.isSearchEnabled()) return;
    this.isLoading.set(true);

    this.billingService.getInvoiceHistory({
      startDate: this.startDate(),
      endDate: this.endDate(),
      patientIds: this.selectedPatients().length > 0 ? this.selectedPatients() : null,
      includeCompleted: this.includeCompleted()
    }).subscribe({
      next: (results) => {
        this.historyResults.set(results);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching billing history', err);
        this.isLoading.set(false);
      }
    });
  }

  viewInvoiceInfo(invoiceId: number) {
    this.selectedInvoiceId.set(invoiceId);
  }
}