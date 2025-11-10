// ADPA API Integration Module

class ADPAApiClient {
    constructor(baseUrl = '/api') {
        this.baseUrl = baseUrl;
        this.token = localStorage.getItem('adpa_token');
    }

    // Helper method to get authentication headers
    getHeaders() {
        const headers = {
            'Content-Type': 'application/json',
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        return headers;
    }

    // Generic API call method
    async apiCall(method, endpoint, data = null) {
        const url = `${this.baseUrl}${endpoint}`;
        
        const config = {
            method: method,
            headers: this.getHeaders(),
        };

        if (data && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
            config.body = JSON.stringify(data);
        }

        try {
            const response = await fetch(url, config);
            
            if (!response.ok) {
                if (response.status === 401) {
                    // Token expired or invalid
                    this.handleAuthenticationError();
                    throw new Error('Authentication failed');
                }
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            
            return await response.text();
        } catch (error) {
            console.error(`API call failed: ${method} ${endpoint}`, error);
            throw error;
        }
    }

    handleAuthenticationError() {
        localStorage.removeItem('adpa_token');
        this.token = null;
        if (window.dashboard) {
            window.dashboard.logout();
        }
    }

    // Authentication API calls
    async login(credentials) {
        return this.apiCall('POST', '/auth/login', credentials);
    }

    async validateToken() {
        return this.apiCall('GET', '/auth/validate');
    }

    async refreshToken() {
        return this.apiCall('POST', '/auth/refresh');
    }

    // SecurityConfiguration API calls
    
    // Security Policies
    async getSecurityPolicies() {
        return this.apiCall('GET', '/SecurityConfiguration/SecurityPolicies');
    }

    async getSecurityPolicy(id) {
        return this.apiCall('GET', `/SecurityConfiguration/SecurityPolicies/${id}`);
    }

    async createSecurityPolicy(policy) {
        return this.apiCall('POST', '/SecurityConfiguration/SecurityPolicies', policy);
    }

    async updateSecurityPolicy(id, policy) {
        return this.apiCall('PUT', `/SecurityConfiguration/SecurityPolicies/${id}`, policy);
    }

    async deleteSecurityPolicy(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/SecurityPolicies/${id}`);
    }

    // Access Controls
    async getAccessControls() {
        return this.apiCall('GET', '/SecurityConfiguration/AccessControls');
    }

    async getAccessControl(id) {
        return this.apiCall('GET', `/SecurityConfiguration/AccessControls/${id}`);
    }

    async createAccessControl(control) {
        return this.apiCall('POST', '/SecurityConfiguration/AccessControls', control);
    }

    async updateAccessControl(id, control) {
        return this.apiCall('PUT', `/SecurityConfiguration/AccessControls/${id}`, control);
    }

    async deleteAccessControl(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/AccessControls/${id}`);
    }

    // Authentication Configurations
    async getAuthenticationConfigurations() {
        return this.apiCall('GET', '/SecurityConfiguration/AuthenticationConfigurations');
    }

    async getAuthenticationConfiguration(id) {
        return this.apiCall('GET', `/SecurityConfiguration/AuthenticationConfigurations/${id}`);
    }

    async createAuthenticationConfiguration(config) {
        return this.apiCall('POST', '/SecurityConfiguration/AuthenticationConfigurations', config);
    }

    async updateAuthenticationConfiguration(id, config) {
        return this.apiCall('PUT', `/SecurityConfiguration/AuthenticationConfigurations/${id}`, config);
    }

    async deleteAuthenticationConfiguration(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/AuthenticationConfigurations/${id}`);
    }

    // Authorization Rules
    async getAuthorizationRules() {
        return this.apiCall('GET', '/SecurityConfiguration/AuthorizationRules');
    }

    async getAuthorizationRule(id) {
        return this.apiCall('GET', `/SecurityConfiguration/AuthorizationRules/${id}`);
    }

    async createAuthorizationRule(rule) {
        return this.apiCall('POST', '/SecurityConfiguration/AuthorizationRules', rule);
    }

    async updateAuthorizationRule(id, rule) {
        return this.apiCall('PUT', `/SecurityConfiguration/AuthorizationRules/${id}`, rule);
    }

    async deleteAuthorizationRule(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/AuthorizationRules/${id}`);
    }

    // Encryption Settings
    async getEncryptionSettings() {
        return this.apiCall('GET', '/SecurityConfiguration/EncryptionSettings');
    }

    async getEncryptionSetting(id) {
        return this.apiCall('GET', `/SecurityConfiguration/EncryptionSettings/${id}`);
    }

    async createEncryptionSetting(setting) {
        return this.apiCall('POST', '/SecurityConfiguration/EncryptionSettings', setting);
    }

    async updateEncryptionSetting(id, setting) {
        return this.apiCall('PUT', `/SecurityConfiguration/EncryptionSettings/${id}`, setting);
    }

    async deleteEncryptionSetting(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/EncryptionSettings/${id}`);
    }

    // API Security Configurations
    async getApiSecurityConfigurations() {
        return this.apiCall('GET', '/SecurityConfiguration/ApiSecurityConfigurations');
    }

    async getApiSecurityConfiguration(id) {
        return this.apiCall('GET', `/SecurityConfiguration/ApiSecurityConfigurations/${id}`);
    }

    async createApiSecurityConfiguration(config) {
        return this.apiCall('POST', '/SecurityConfiguration/ApiSecurityConfigurations', config);
    }

    async updateApiSecurityConfiguration(id, config) {
        return this.apiCall('PUT', `/SecurityConfiguration/ApiSecurityConfigurations/${id}`, config);
    }

    async deleteApiSecurityConfiguration(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/ApiSecurityConfigurations/${id}`);
    }

    // Security Incidents
    async getSecurityIncidents() {
        return this.apiCall('GET', '/SecurityConfiguration/SecurityIncidents');
    }

    async getSecurityIncident(id) {
        return this.apiCall('GET', `/SecurityConfiguration/SecurityIncidents/${id}`);
    }

    async createSecurityIncident(incident) {
        return this.apiCall('POST', '/SecurityConfiguration/SecurityIncidents', incident);
    }

    async updateSecurityIncident(id, incident) {
        return this.apiCall('PUT', `/SecurityConfiguration/SecurityIncidents/${id}`, incident);
    }

    async deleteSecurityIncident(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/SecurityIncidents/${id}`);
    }

    // Audit Logs
    async getAuditLogs() {
        return this.apiCall('GET', '/SecurityConfiguration/AuditLogs');
    }

    async getAuditLog(id) {
        return this.apiCall('GET', `/SecurityConfiguration/AuditLogs/${id}`);
    }

    async createAuditLog(log) {
        return this.apiCall('POST', '/SecurityConfiguration/AuditLogs', log);
    }

    async updateAuditLog(id, log) {
        return this.apiCall('PUT', `/SecurityConfiguration/AuditLogs/${id}`, log);
    }

    async deleteAuditLog(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/AuditLogs/${id}`);
    }

    // Compliance Reports
    async getComplianceReports() {
        return this.apiCall('GET', '/SecurityConfiguration/ComplianceReports');
    }

    async getComplianceReport(id) {
        return this.apiCall('GET', `/SecurityConfiguration/ComplianceReports/${id}`);
    }

    async createComplianceReport(report) {
        return this.apiCall('POST', '/SecurityConfiguration/ComplianceReports', report);
    }

    async updateComplianceReport(id, report) {
        return this.apiCall('PUT', `/SecurityConfiguration/ComplianceReports/${id}`, report);
    }

    async deleteComplianceReport(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/ComplianceReports/${id}`);
    }

    // Security Monitoring
    async getSecurityMonitoringConfigurations() {
        return this.apiCall('GET', '/SecurityConfiguration/SecurityMonitoringConfigurations');
    }

    async getSecurityMonitoringConfiguration(id) {
        return this.apiCall('GET', `/SecurityConfiguration/SecurityMonitoringConfigurations/${id}`);
    }

    async createSecurityMonitoringConfiguration(config) {
        return this.apiCall('POST', '/SecurityConfiguration/SecurityMonitoringConfigurations', config);
    }

    async updateSecurityMonitoringConfiguration(id, config) {
        return this.apiCall('PUT', `/SecurityConfiguration/SecurityMonitoringConfigurations/${id}`, config);
    }

    async deleteSecurityMonitoringConfiguration(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/SecurityMonitoringConfigurations/${id}`);
    }

    // System Configurations
    async getSystemConfigurations() {
        return this.apiCall('GET', '/SecurityConfiguration/SystemConfigurations');
    }

    async getSystemConfiguration(id) {
        return this.apiCall('GET', `/SecurityConfiguration/SystemConfigurations/${id}`);
    }

    async createSystemConfiguration(config) {
        return this.apiCall('POST', '/SecurityConfiguration/SystemConfigurations', config);
    }

    async updateSystemConfiguration(id, config) {
        return this.apiCall('PUT', `/SecurityConfiguration/SystemConfigurations/${id}`, config);
    }

    async deleteSystemConfiguration(id) {
        return this.apiCall('DELETE', `/SecurityConfiguration/SystemConfigurations/${id}`);
    }

    // Dashboard and Statistics
    async getDashboardStats() {
        try {
            // Get counts for each entity type
            const [
                securityPolicies,
                accessControls,
                authConfigs,
                authRules,
                encryptionSettings,
                apiSecurityConfigs,
                securityIncidents,
                auditLogs,
                complianceReports,
                monitoringConfigs,
                systemConfigs
            ] = await Promise.all([
                this.getSecurityPolicies(),
                this.getAccessControls(),
                this.getAuthenticationConfigurations(),
                this.getAuthorizationRules(),
                this.getEncryptionSettings(),
                this.getApiSecurityConfigurations(),
                this.getSecurityIncidents(),
                this.getAuditLogs(),
                this.getComplianceReports(),
                this.getSecurityMonitoringConfigurations(),
                this.getSystemConfigurations()
            ]);

            return {
                securityPolicies: Array.isArray(securityPolicies) ? securityPolicies.length : 0,
                accessControls: Array.isArray(accessControls) ? accessControls.length : 0,
                authenticationConfigurations: Array.isArray(authConfigs) ? authConfigs.length : 0,
                authorizationRules: Array.isArray(authRules) ? authRules.length : 0,
                encryptionSettings: Array.isArray(encryptionSettings) ? encryptionSettings.length : 0,
                apiSecurityConfigurations: Array.isArray(apiSecurityConfigs) ? apiSecurityConfigs.length : 0,
                securityIncidents: Array.isArray(securityIncidents) ? securityIncidents.length : 0,
                auditLogs: Array.isArray(auditLogs) ? auditLogs.length : 0,
                complianceReports: Array.isArray(complianceReports) ? complianceReports.length : 0,
                monitoringConfigurations: Array.isArray(monitoringConfigs) ? monitoringConfigs.length : 0,
                systemConfigurations: Array.isArray(systemConfigs) ? systemConfigs.length : 0,
                totalItems: [
                    securityPolicies, accessControls, authConfigs, authRules, encryptionSettings,
                    apiSecurityConfigs, securityIncidents, auditLogs, complianceReports,
                    monitoringConfigs, systemConfigs
                ].reduce((total, arr) => total + (Array.isArray(arr) ? arr.length : 0), 0)
            };
        } catch (error) {
            console.error('Error loading dashboard statistics:', error);
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
                totalItems: 0
            };
        }
    }

    async getRecentSecurityEvents() {
        try {
            // Get recent audit logs and security incidents
            const [auditLogs, securityIncidents] = await Promise.all([
                this.getAuditLogs(),
                this.getSecurityIncidents()
            ]);

            const events = [];

            // Add recent audit logs
            if (Array.isArray(auditLogs)) {
                auditLogs.slice(0, 5).forEach(log => {
                    events.push({
                        type: 'audit',
                        title: log.action || 'Audit Event',
                        description: log.description || 'No description available',
                        timestamp: log.timestamp || log.createdAt,
                        severity: 'info',
                        user: log.userId || log.username || 'System'
                    });
                });
            }

            // Add recent security incidents
            if (Array.isArray(securityIncidents)) {
                securityIncidents.slice(0, 5).forEach(incident => {
                    events.push({
                        type: 'incident',
                        title: incident.title || 'Security Incident',
                        description: incident.description || 'No description available',
                        timestamp: incident.detectedAt || incident.createdAt,
                        severity: incident.severity || 'medium',
                        status: incident.status || 'open'
                    });
                });
            }

            // Sort by timestamp (most recent first)
            return events.sort((a, b) => {
                const dateA = new Date(a.timestamp);
                const dateB = new Date(b.timestamp);
                return dateB - dateA;
            }).slice(0, 10);

        } catch (error) {
            console.error('Error loading recent security events:', error);
            return [];
        }
    }

    async getSystemHealth() {
        try {
            // Mock system health data - in real implementation, this would call actual health endpoints
            return {
                status: 'healthy',
                uptime: '99.9%',
                lastBackup: new Date().toISOString(),
                databaseStatus: 'connected',
                apiStatus: 'operational',
                securityStatus: 'secure'
            };
        } catch (error) {
            console.error('Error loading system health:', error);
            return {
                status: 'unknown',
                uptime: 'N/A',
                lastBackup: 'Unknown',
                databaseStatus: 'unknown',
                apiStatus: 'unknown',
                securityStatus: 'unknown'
            };
        }
    }

    // Bulk operations
    async bulkDelete(entityType, ids) {
        return this.apiCall('POST', `/SecurityConfiguration/${entityType}/bulk-delete`, { ids });
    }

    async bulkUpdate(entityType, updates) {
        return this.apiCall('POST', `/SecurityConfiguration/${entityType}/bulk-update`, updates);
    }

    // Export/Import operations
    async exportData(entityType) {
        return this.apiCall('GET', `/SecurityConfiguration/${entityType}/export`);
    }

    async importData(entityType, data) {
        return this.apiCall('POST', `/SecurityConfiguration/${entityType}/import`, data);
    }

    // Search operations
    async searchEntities(entityType, query, filters = {}) {
        const params = new URLSearchParams({
            q: query,
            ...filters
        });
        
        return this.apiCall('GET', `/SecurityConfiguration/${entityType}/search?${params}`);
    }

    // Validation operations
    async validateEntity(entityType, data) {
        return this.apiCall('POST', `/SecurityConfiguration/${entityType}/validate`, data);
    }

    // Update token
    setToken(token) {
        this.token = token;
        if (token) {
            localStorage.setItem('adpa_token', token);
        } else {
            localStorage.removeItem('adpa_token');
        }
    }
}

// Create global API client instance
window.apiClient = new ADPAApiClient();

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ADPAApiClient;
}