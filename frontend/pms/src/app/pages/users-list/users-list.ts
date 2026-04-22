import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserModal } from '../user-modal/user-modal';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-users-list',
  imports: [CommonModule, FormsModule, UserModal],
  templateUrl: './users-list.html',
  styleUrl: './users-list.css',
})
export class UsersList implements OnInit {
  users = signal<UserInterface[]>([]);
  currentUser: UserInterface = {
    id: 0,
    userName: '',
    password: '',
    name: '',
    email: '',
    isActive: false,
    role: '',
    creationDate: new Date()
  };
  showModal = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.userService.getUsers().subscribe((users) => {
      this.users.set(users);
    });
  }

  openCreateModal(): void {
    this.isEditing.set(false);
    this.currentUser = {
      id: 0,
      userName: '',
      password: '',
      name: '',
      email: '',
      isActive: false,
      role: '',
      creationDate: new Date()
    };
    this.showModal.set(true);
  }

  openEditModal(user: UserInterface): void {
    this.isEditing.set(true);
    this.currentUser = { ...user };
    this.showModal.set(true);
  }

  saveUser(user: UserInterface): void {
    if (this.isEditing()) {
      // Handle update
      alert(`User ${user.userName} updated successfully!`);
      // TODO: Call userService.updateUser(user).subscribe(...)
    } else {
      // Handle create
      alert(`User ${user.userName} created successfully!`);
      // TODO: Call userService.createUser(user).subscribe(...)
    }
    this.closeModal();
    this.loadUsers();
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  deleteUser(id?: number): void {
    if (id && confirm('Are you sure to remove this user? This action cannot be undone.')) {
      alert(`User with ID ${id} deleted successfully!`);
      // TODO: Call userService.deleteUser(id).subscribe(() => this.loadUsers())
    }
  }
}
