import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-users-list',
  imports: [CommonModule, FormsModule],
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
    this.userService.getUsers().subscribe((users) => {
      this.users.set(users);
    });
  }

  openCreateModal(): void {
    this.isEditing.set(false);
    alert('Opening create user modal');
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
    //this.currentUser = this.getEmptyUser();
  }

  openEditModal(user: UserInterface): void {
    this.isEditing.set(true);
    this.currentUser = { ...user };

    alert(`Editing user: ${user.userName}`);

    this.showModal.set(true);
  }

  deleteUser(id?: number): void {
    if (id && confirm('Are you sure to remove this user? This action cannot be undone.')) {
      alert(`User with ID ${id} deleted successfully!`);
      // Here you would call the service to delete the user and refresh the list
      // this.userService.deleteUser(id).subscribe(() => this.loadUsers()
    }
  }
}
