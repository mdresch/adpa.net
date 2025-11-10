// ADPA Administrative Dashboard JavaScript

class ADPADashboard {
    constructor() {
        this.apiBaseUrl = '/api';
        this.currentUser = null;
        this.isAuthenticated = false;
        this.currentPage = 'dashboard';
        
        // Initialize dashboard
        this.init();
    }

    async init() {
        // Show loading screen
        this.showLoading();
        
        // Check authentication
        await this.checkAuth();
        
        if (this.isAuthenticated) {
            this.showDashboard();
            this.initializeComponents();
            this.loadDashboardData();
        } else {
            this.showLogin();
        }
        
        this.hideLoading();
    }

    // Authentication Methods
    async checkAuth() {
        const token = localStorage.getItem('adpa_token');
        if (!token) {
            this.isAuthenticated = false;
            return;
        }

        try {
            const response = await this.apiCall('GET', '/auth/validate', null, token);
            if (response.success) {
                this.currentUser = response.user;
                this.isAuthenticated = true;
            } else {
                localStorage.removeItem('adpa_token');
                this.isAuthenticated = false;
            }
        } catch (error) {
            console.error('Auth check failed:', error);
            localStorage.removeItem('adpa_token');
            this.isAuthenticated = false;
        }
    }

    async login(username, password) {
        try {
            const response = await this.apiCall('POST', '/auth/login', {
                username: username,
                password: password
            });

            if (response.success) {
                localStorage.setItem('adpa_token', response.token);
                this.currentUser = response.user;
                this.isAuthenticated = true;
                this.showDashboard();
                this.initializeComponents();
                this.loadDashboardData();
                return { success: true };
            } else {
                return { success: false, message: response.message || 'Login failed' };
            }
        } catch (error) {
            console.error('Login error:', error);
            return { success: false, message: 'Login failed. Please try again.' };
        }
    }

    logout() {
        localStorage.removeItem('adpa_token');
        this.currentUser = null;
        this.isAuthenticated = false;
        this.showLogin();
    }

    // UI State Management
    showLoading() {
        document.getElementById('loadingScreen').classList.remove('hidden');
        document.getElementById('loginScreen').classList.add('hidden');
        document.getElementById('mainDashboard').classList.add('hidden');
    }

    hideLoading() {
        document.getElementById('loadingScreen').classList.add('hidden');
    }

    showLogin() {
        document.getElementById('loginScreen').classList.remove('hidden');
        document.getElementById('mainDashboard').classList.add('hidden');
    }

    showDashboard() {
        document.getElementById('loginScreen').classList.add('hidden');
        document.getElementById('mainDashboard').classList.remove('hidden');
        
        // Update user info in header
        if (this.currentUser) {
            document.getElementById('userName').textContent = this.currentUser.name || this.currentUser.username;
            document.getElementById('userRole').textContent = this.currentUser.role || 'Administrator';
            document.getElementById('userAvatar').textContent = (this.currentUser.name || this.currentUser.username).charAt(0).toUpperCase();
            document.getElementById('headerUserName').textContent = this.currentUser.name || this.currentUser.username;
        }
    }

    // Component Initialization
    initializeComponents() {
        this.initializeNavigation();
        this.initializeDropdowns();
        this.initializeSidebar();
        this.initializeSearch();
        this.initializeNotifications();
    }

    initializeNavigation() {
        // Navigation link click handlers
        document.querySelectorAll('.nav-link').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const page = link.getAttribute('data-page');
                if (page) {
                    this.navigateToPage(page);
                }
            });
        });

        // Set dashboard as active by default
        this.setActiveNavItem('dashboard');
    }

    initializeDropdowns() {
        // User dropdown
        const userBtn = document.getElementById('userBtn');
        const userDropdown = document.getElementById('userDropdown');
        
        userBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            userDropdown.classList.toggle('active');
            document.getElementById('notificationDropdown').classList.remove('active');
        });

        // Notification dropdown
        const notificationBtn = document.getElementById('notificationBtn');
        const notificationDropdown = document.getElementById('notificationDropdown');
        
        notificationBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            notificationDropdown.classList.toggle('active');
            userDropdown.classList.remove('active');
        });

        // Close dropdowns when clicking outside
        document.addEventListener('click', () => {
            userDropdown.classList.remove('active');
            notificationDropdown.classList.remove('active');
        });

        // Logout handler
        document.getElementById('logoutBtn').addEventListener('click', (e) => {
            e.preventDefault();
            this.logout();
        });
    }

    initializeSidebar() {
        const sidebarToggle = document.getElementById('sidebarToggle');
        const sidebar = document.getElementById('sidebar');
        
        sidebarToggle.addEventListener('click', () => {
            sidebar.classList.toggle('active');
        });
    }

    initializeSearch() {
        const searchInput = document.getElementById('searchInput');
        let searchTimeout;

        searchInput.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                this.performSearch(e.target.value);
            }, 300);
        });
    }

    initializeNotifications() {
        // Load initial notifications
        this.loadNotifications();
        
        // Set up periodic refresh
        setInterval(() => {
            this.loadNotifications();
        }, 30000); // Refresh every 30 seconds
    }

    // Navigation Methods
    navigateToPage(page) {
        this.currentPage = page;
        this.setActiveNavItem(page);
        this.loadPageContent(page);
    }

    setActiveNavItem(page) {
        // Remove active class from all nav links
        document.querySelectorAll('.nav-link').forEach(link => {
            link.classList.remove('active');
        });

        // Add active class to current page link
        const activeLink = document.querySelector(`[data-page="${page}"]`);
        if (activeLink) {
            activeLink.classList.add('active');
        }
    }

    async loadPageContent(page) {
        const mainContent = document.getElementById('mainContent');
        
        // Show loading state
        mainContent.innerHTML = '<div class="text-center"><div class="loading-spinner"></div><p>Loading...</p></div>';

        try {
            let content = '';
            
            switch (page) {
                case 'dashboard':
                    content = await this.loadDashboardContent();
                    break;
                case 'security-policies':
                    content = await this.loadSecurityPoliciesContent();
                    break;
                case 'access-controls':
                    content = await this.loadAccessControlsContent();
                    break;
                case 'encryption-settings':
                    content = await this.loadEncryptionSettingsContent();
                    break;
                case 'authentication-methods':
                    content = await this.loadAuthenticationMethodsContent();
                    break;
                case 'authorization-rules':
                    content = await this.loadAuthorizationRulesContent();
                    break;
                case 'api-security':
                    content = await this.loadApiSecurityContent();
                    break;
                case 'audit-logs':
                    content = await this.loadAuditLogsContent();
                    break;
                case 'security-monitoring':
                    content = await this.loadSecurityMonitoringContent();
                    break;
                case 'compliance':
                    content = await this.loadComplianceContent();
                    break;
                case 'system-monitoring':
                    content = await this.loadSystemMonitoringContent();
                    break;
                default:
                    content = '<div class="text-center"><h3>Page not found</h3><p>The requested page could not be found.</p></div>';
            }

            mainContent.innerHTML = content;
        } catch (error) {
            console.error('Error loading page content:', error);
            mainContent.innerHTML = '<div class="text-center text-danger"><h3>Error</h3><p>Failed to load page content. Please try again.</p></div>';
        }
    }

    // Dashboard Content Loaders
    async loadDashboardContent() {
        const stats = await this.loadDashboardStats();
        
        return `
            <div class="page-container fade-in">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h1>Security Dashboard</h1>
                    <div class="d-flex gap-2">
                        <button class="btn btn-outline-primary btn-sm">
                            <i class="fas fa-download"></i> Export Report
                        </button>
                        <button class="btn btn-primary btn-sm">
                            <i class="fas fa-plus"></i> Quick Setup
                        </button>
                    </div>
                </div>

                <div class="dashboard-grid">
                    <div class="dashboard-card">
                        <div class="card-header">
                            <h3 class="card-title">Security Policies</h3>
                            <div class="card-icon security">
                                <i class="fas fa-shield-alt"></i>
                            </div>
                        </div>
                        <div class="card-value">${stats.securityPolicies || 0}</div>
                        <div class="card-description">Active security policies</div>
                    </div>

                    <div class="dashboard-card">
                        <div class="card-header">
                            <h3 class="card-title">Access Controls</h3>
                            <div class="card-icon info">
                                <i class="fas fa-key"></i>
                            </div>
                        </div>
                        <div class="card-value">${stats.accessControls || 0}</div>
                        <div class="card-description">Configured access controls</div>
                    </div>

                    <div class="dashboard-card">
                        <div class="card-header">
                            <h3 class="card-title">Active Sessions</h3>
                            <div class="card-icon success">
                                <i class="fas fa-users"></i>
                            </div>
                        </div>
                        <div class="card-value">${stats.activeSessions || 0}</div>
                        <div class="card-description">Current user sessions</div>
                    </div>

                    <div class="dashboard-card">
                        <div class="card-header">
                            <h3 class="card-title">Security Alerts</h3>
                            <div class="card-icon ${stats.securityAlerts > 0 ? 'warning' : 'success'}">
                                <i class="fas fa-exclamation-triangle"></i>
                            </div>
                        </div>
                        <div class="card-value">${stats.securityAlerts || 0}</div>
                        <div class="card-description">Security alerts requiring attention</div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-8">
                        <div class="dashboard-card">
                            <div class="card-header">
                                <h3 class="card-title">Recent Security Events</h3>
                                <button class="btn btn-outline-primary btn-sm">View All</button>
                            </div>
                            <div id="recentEvents">Loading events...</div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="dashboard-card">
                            <div class="card-header">
                                <h3 class="card-title">System Status</h3>
                            </div>
                            <div id="systemStatus">Loading status...</div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    async loadSecurityPoliciesContent() {
        try {
            const policies = await window.apiClient.getSecurityPolicies();
            
            return `
                <div class="page-container fade-in">
                    <div class="d-flex justify-content-between align-items-center mb-4">
                        <h1>Security Policies</h1>
                        <div class="d-flex gap-2">
                            <button class="btn btn-outline-primary btn-sm" onclick="dashboard.exportData('SecurityPolicies')">
                                <i class="fas fa-download"></i> Export
                            </button>
                            <button class="btn btn-primary" onclick="dashboard.showCreateModal('security-policy')">
                                <i class="fas fa-plus"></i> New Policy
                            </button>
                        </div>
                    </div>
                    
                    <div class="dashboard-card">
                        <div class="card-header">
                            <h3 class="card-title">Security Policies (${Array.isArray(policies) ? policies.length : 0})</h3>
                            <div class="d-flex gap-2">
                                <input type="text" class="form-control" placeholder="Search policies..." 
                                       onkeyup="dashboard.filterTable(this.value, 'securityPoliciesTable')">
                            </div>
                        </div>
                        <div class="table-responsive">
                            <table class="data-table">
                                <thead>
                                    <tr>
                                        <th>ID</th>
                                        <th>Name</th>
                                        <th>Description</th>
                                        <th>Type</th>
                                        <th>Status</th>
                                        <th>Created</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody id="securityPoliciesTable">
                                    ${this.renderSecurityPoliciesTable(policies)}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            `;
        } catch (error) {
            console.error('Error loading security policies:', error);
            return `
                <div class="page-container fade-in">
                    <h1>Security Policies</h1>
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle"></i>
                        Failed to load security policies. Please try again later.
                    </div>
                </div>
            `;
        }
    }

    // API Helper Methods
    async apiCall(method, endpoint, data = null, token = null) {
        const url = this.apiBaseUrl + endpoint;
        const headers = {
            'Content-Type': 'application/json',
        };

        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        } else if (this.isAuthenticated) {
            const storedToken = localStorage.getItem('adpa_token');
            if (storedToken) {
                headers['Authorization'] = `Bearer ${storedToken}`;
            }
        }

        const config = {
            method: method,
            headers: headers,
        };

        if (data && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
            config.body = JSON.stringify(data);
        }

        try {
            const response = await fetch(url, config);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('API call failed:', error);
            throw error;
        }
    }

    async loadDashboardStats() {
        try {
            // Use API client to get real statistics
            const stats = await window.apiClient.getDashboardStats();
            
            // Also get recent security events count for alerts
            const recentEvents = await window.apiClient.getRecentSecurityEvents();
            const securityAlerts = recentEvents.filter(event => 
                event.type === 'incident' && event.status === 'open'
            ).length;
            
            return {
                ...stats,
                activeSessions: 24, // This would come from session management API
                securityAlerts: securityAlerts
            };
        } catch (error) {
            console.error('Error loading dashboard stats:', error);
            // Return mock data as fallback
            return {
                securityPolicies: 0,
                accessControls: 0,
                authenticationConfigurations: 0,
                authorizationRules: 0,
                encryptionSettings: 0,
                apiSecurityConfigurations: 0,
                securityIncidents: 0,
                auditLogs: 0,
                complianceReports: 0,
                monitoringConfigurations: 0,
                systemConfigurations: 0,
                totalItems: 0,
                activeSessions: 0,
                securityAlerts: 0
            };
        }
    }

    async loadNotifications() {
        try {
            // Mock notifications - replace with actual API call
            const notifications = [
                { id: 1, title: 'Security Policy Updated', message: 'Password policy has been updated', time: '5 minutes ago', read: false },
                { id: 2, title: 'New User Registered', message: 'John Doe has registered', time: '1 hour ago', read: true },
                { id: 3, title: 'System Backup Complete', message: 'Daily backup completed successfully', time: '2 hours ago', read: true }
            ];

            this.updateNotificationUI(notifications);
        } catch (error) {
            console.error('Error loading notifications:', error);
        }
    }

    updateNotificationUI(notifications) {
        const unreadCount = notifications.filter(n => !n.read).length;
        const countElement = document.getElementById('notificationCount');
        
        if (unreadCount > 0) {
            countElement.textContent = unreadCount;
            countElement.style.display = 'flex';
        } else {
            countElement.style.display = 'none';
        }

        // Update notification dropdown content
        const notificationList = document.getElementById('notificationList');
        if (notificationList) {
            notificationList.innerHTML = notifications.map(notification => `
                <div class="notification-item ${notification.read ? 'read' : 'unread'}">
                    <div class="notification-content">
                        <h5>${notification.title}</h5>
                        <p>${notification.message}</p>
                        <span class="notification-time">${notification.time}</span>
                    </div>
                </div>
            `).join('');
        }
    }

    performSearch(query) {
        if (query.length < 2) return;
        
        console.log('Performing search for:', query);
        // Implement search functionality
    }

    // Table Rendering Methods
    renderSecurityPoliciesTable(policies) {
        if (!policies || policies.length === 0) {
            return '<tr><td colspan="7" class="text-center">No security policies found</td></tr>';
        }

        return policies.map(policy => `
            <tr>
                <td>${policy.id || 'N/A'}</td>
                <td>${policy.name || policy.policyName || 'N/A'}</td>
                <td>${policy.description || 'N/A'}</td>
                <td>${policy.policyType || 'General'}</td>
                <td><span class="status-badge ${policy.isActive ? 'active' : 'inactive'}">${policy.isActive ? 'Active' : 'Inactive'}</span></td>
                <td>${policy.createdAt ? formatDate(policy.createdAt) : 'N/A'}</td>
                <td>
                    <div class="action-buttons">
                        <button class="btn-action btn-edit" onclick="dashboard.editItem('SecurityPolicies', ${policy.id})" title="Edit Policy">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn-action btn-view" onclick="dashboard.viewItem('SecurityPolicies', ${policy.id})" title="View Details">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn-action btn-delete" onclick="dashboard.deleteItem('SecurityPolicies', ${policy.id})" title="Delete Policy">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `).join('');
    }

    // Modal and Form Methods
    showCreateModal(type) {
        console.log('Show create modal for:', type);
        // Implement modal functionality
    }

    editItem(type, id) {
        console.log('Edit item:', type, id);
        // Implement edit functionality
    }

    async deleteItem(type, id) {
        if (confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
            try {
                // Show loading state
                const button = event.target.closest('button');
                button.disabled = true;
                button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
                
                // Call appropriate delete method based on type
                switch (type) {
                    case 'SecurityPolicies':
                        await window.apiClient.deleteSecurityPolicy(id);
                        break;
                    case 'AccessControls':
                        await window.apiClient.deleteAccessControl(id);
                        break;
                    case 'AuthenticationConfigurations':
                        await window.apiClient.deleteAuthenticationConfiguration(id);
                        break;
                    case 'AuthorizationRules':
                        await window.apiClient.deleteAuthorizationRule(id);
                        break;
                    case 'EncryptionSettings':
                        await window.apiClient.deleteEncryptionSetting(id);
                        break;
                    case 'ApiSecurityConfigurations':
                        await window.apiClient.deleteApiSecurityConfiguration(id);
                        break;
                    case 'SecurityIncidents':
                        await window.apiClient.deleteSecurityIncident(id);
                        break;
                    case 'AuditLogs':
                        await window.apiClient.deleteAuditLog(id);
                        break;
                    case 'ComplianceReports':
                        await window.apiClient.deleteComplianceReport(id);
                        break;
                    case 'SecurityMonitoringConfigurations':
                        await window.apiClient.deleteSecurityMonitoringConfiguration(id);
                        break;
                    case 'SystemConfigurations':
                        await window.apiClient.deleteSystemConfiguration(id);
                        break;
                    default:
                        throw new Error(`Unknown entity type: ${type}`);
                }
                
                // Refresh the current page to show updated data
                this.loadPageContent(this.currentPage);
                
                // Show success message
                this.showNotification('Item deleted successfully', 'success');
                
            } catch (error) {
                console.error('Delete failed:', error);
                this.showNotification('Failed to delete item. Please try again.', 'error');
                
                // Restore button state
                const button = event.target.closest('button');
                button.disabled = false;
                button.innerHTML = '<i class="fas fa-trash"></i>';
            }
        }
    }

    viewItem(type, id) {
        console.log('View item:', type, id);
        // Implement view functionality - could open a modal with detailed information
        this.showItemModal(type, id, 'view');
    }

    async exportData(entityType) {
        try {
            const data = await window.apiClient.exportData(entityType);
            
            // Create downloadable file
            const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${entityType}_export_${new Date().toISOString().split('T')[0]}.json`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
            
            this.showNotification('Data exported successfully', 'success');
        } catch (error) {
            console.error('Export failed:', error);
            this.showNotification('Failed to export data. Please try again.', 'error');
        }
    }

    filterTable(searchTerm, tableId) {
        const table = document.getElementById(tableId);
        if (!table) return;
        
        const rows = table.getElementsByTagName('tr');
        searchTerm = searchTerm.toLowerCase();
        
        for (let i = 0; i < rows.length; i++) {
            const row = rows[i];
            const cells = row.getElementsByTagName('td');
            let found = false;
            
            for (let j = 0; j < cells.length; j++) {
                const cellText = cells[j].textContent || cells[j].innerText;
                if (cellText.toLowerCase().includes(searchTerm)) {
                    found = true;
                    break;
                }
            }
            
            row.style.display = found ? '' : 'none';
        }
    }

    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-message">${message}</span>
                <button class="notification-close" onclick="this.parentElement.parentElement.remove()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        
        // Add to page
        document.body.appendChild(notification);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentElement) {
                notification.remove();
            }
        }, 5000);
    }

    async showItemModal(type, id, mode = 'edit') {
        // This would show a modal with item details for editing or viewing
        console.log(`Show ${mode} modal for ${type} item ${id}`);
        // Implementation would create and show a modal dialog
    }

    // Access Controls content loader
    async loadAccessControlsContent() {
        try {
            const accessControls = await window.apiClient.getAccessControls();
            
            return `
                <div class="page-container fade-in">
                    <div class="d-flex justify-content-between align-items-center mb-4">
                        <h1>Access Controls</h1>
                        <div class="d-flex gap-2">
                            <button class="btn btn-outline-primary btn-sm" onclick="dashboard.exportData('AccessControls')">
                                <i class="fas fa-download"></i> Export
                            </button>
                            <button class="btn btn-primary" onclick="dashboard.showCreateModal('access-control')">
                                <i class="fas fa-plus"></i> New Access Control
                            </button>
                        </div>
                    </div>
                    
                    <div class="dashboard-card">
                        <div class="card-header">
                            <h3 class="card-title">Access Controls (${Array.isArray(accessControls) ? accessControls.length : 0})</h3>
                            <div class="d-flex gap-2">
                                <input type="text" class="form-control" placeholder="Search access controls..." 
                                       onkeyup="dashboard.filterTable(this.value, 'accessControlsTable')">
                            </div>
                        </div>
                        <div class="table-responsive">
                            <table class="data-table">
                                <thead>
                                    <tr>
                                        <th>ID</th>
                                        <th>Name</th>
                                        <th>Resource</th>
                                        <th>Permission Level</th>
                                        <th>Status</th>
                                        <th>Created</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody id="accessControlsTable">
                                    ${this.renderAccessControlsTable(accessControls)}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            `;
        } catch (error) {
            console.error('Error loading access controls:', error);
            return `
                <div class="page-container fade-in">
                    <h1>Access Controls</h1>
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle"></i>
                        Failed to load access controls. Please try again later.
                    </div>
                </div>
            `;
        }
    }

    renderAccessControlsTable(accessControls) {
        if (!accessControls || accessControls.length === 0) {
            return '<tr><td colspan="7" class="text-center">No access controls found</td></tr>';
        }

        return accessControls.map(control => `
            <tr>
                <td>${control.id || 'N/A'}</td>
                <td>${control.name || control.controlName || 'N/A'}</td>
                <td>${control.resource || control.resourceName || 'N/A'}</td>
                <td>${control.permissionLevel || control.accessLevel || 'N/A'}</td>
                <td><span class="status-badge ${control.isActive ? 'active' : 'inactive'}">${control.isActive ? 'Active' : 'Inactive'}</span></td>
                <td>${control.createdAt ? formatDate(control.createdAt) : 'N/A'}</td>
                <td>
                    <div class="action-buttons">
                        <button class="btn-action btn-edit" onclick="dashboard.editItem('AccessControls', ${control.id})" title="Edit Control">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn-action btn-view" onclick="dashboard.viewItem('AccessControls', ${control.id})" title="View Details">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn-action btn-delete" onclick="dashboard.deleteItem('AccessControls', ${control.id})" title="Delete Control">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `).join('');
    }

    // Placeholder methods for other content loaders

    async loadEncryptionSettingsContent() {
        return '<div class="page-container fade-in"><h1>Encryption Settings</h1><p>Encryption settings interface coming soon...</p></div>';
    }

    async loadAuthenticationMethodsContent() {
        return '<div class="page-container fade-in"><h1>Authentication Methods</h1><p>Authentication methods interface coming soon...</p></div>';
    }

    async loadAuthorizationRulesContent() {
        return '<div class="page-container fade-in"><h1>Authorization Rules</h1><p>Authorization rules interface coming soon...</p></div>';
    }

    async loadApiSecurityContent() {
        return '<div class="page-container fade-in"><h1>API Security</h1><p>API security interface coming soon...</p></div>';
    }

    async loadAuditLogsContent() {
        return '<div class="page-container fade-in"><h1>Audit Logs</h1><p>Audit logs interface coming soon...</p></div>';
    }

    async loadSecurityMonitoringContent() {
        return '<div class="page-container fade-in"><h1>Security Monitoring</h1><p>Security monitoring interface coming soon...</p></div>';
    }

    async loadComplianceContent() {
        return '<div class="page-container fade-in"><h1>Compliance</h1><p>Compliance interface coming soon...</p></div>';
    }

    async loadSystemMonitoringContent() {
        return '<div class="page-container fade-in"><h1>System Monitoring</h1><p>System monitoring interface coming soon...</p></div>';
    }
}

// Authentication form handler
document.addEventListener('DOMContentLoaded', function() {
    // Initialize dashboard
    window.dashboard = new ADPADashboard();

    // Login form handler
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;
            const errorMessage = document.getElementById('errorMessage');
            
            // Clear previous errors
            errorMessage.style.display = 'none';
            
            // Show loading state
            const submitBtn = e.target.querySelector('button[type="submit"]');
            const originalText = submitBtn.textContent;
            submitBtn.textContent = 'Signing in...';
            submitBtn.disabled = true;
            
            try {
                const result = await dashboard.login(username, password);
                
                if (!result.success) {
                    errorMessage.textContent = result.message;
                    errorMessage.style.display = 'block';
                }
            } catch (error) {
                errorMessage.textContent = 'An error occurred. Please try again.';
                errorMessage.style.display = 'block';
            } finally {
                // Restore button state
                submitBtn.textContent = originalText;
                submitBtn.disabled = false;
            }
        });
    }
});

// Utility functions
function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString();
}

function formatDateTime(dateString) {
    return new Date(dateString).toLocaleString();
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}