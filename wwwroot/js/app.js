/**
 * Attendance Management System - Main JavaScript
 * Handles sidebar, AJAX operations, SweetAlert, and UI interactions
 */

(function() {
    'use strict';

    // ==========================================
    // SIDEBAR MANAGEMENT
    // ==========================================
    
    const SIDEBAR_STATE_KEY = 'sidebarCollapsed';
    const SIDEBAR_SCROLL_KEY = 'sidebarScrollPosition';
    
    function initSidebar() {
        const sidebar = document.querySelector('.sidebar');
        const sidebarNav = document.querySelector('.sidebar-nav');
        const sidebarToggle = document.querySelector('#sidebarToggle');
        const mobileToggle = document.querySelector('.mobile-menu-toggle');
        const overlay = document.querySelector('.sidebar-overlay');
        
        if (!sidebar) return;
        
        // Restore collapsed state from localStorage
        const isCollapsed = localStorage.getItem(SIDEBAR_STATE_KEY) === 'true';
        if (isCollapsed && window.innerWidth > 992) {
            sidebar.classList.add('collapsed');
        }
        
        // Restore scroll position from sessionStorage
        if (sidebarNav) {
            const savedScrollPos = sessionStorage.getItem(SIDEBAR_SCROLL_KEY);
            if (savedScrollPos) {
                sidebarNav.scrollTop = parseInt(savedScrollPos, 10);
            }
            
            // Save scroll position before page unload
            window.addEventListener('beforeunload', function() {
                sessionStorage.setItem(SIDEBAR_SCROLL_KEY, sidebarNav.scrollTop);
            });
            
            // Save scroll position on navigation link clicks
            const navLinks = sidebarNav.querySelectorAll('a');
            navLinks.forEach(link => {
                link.addEventListener('click', function() {
                    sessionStorage.setItem(SIDEBAR_SCROLL_KEY, sidebarNav.scrollTop);
                });
            });
        }
        
        // Desktop toggle
        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function(e) {
                e.preventDefault();
                sidebar.classList.toggle('collapsed');
                localStorage.setItem(SIDEBAR_STATE_KEY, sidebar.classList.contains('collapsed'));
            });
        }
        
        // Mobile toggle
        if (mobileToggle) {
            mobileToggle.addEventListener('click', function() {
                sidebar.classList.toggle('show');
            });
        }
        
        // Overlay click to close
        if (overlay) {
            overlay.addEventListener('click', function() {
                sidebar.classList.remove('show');
            });
        }
        
        // Close sidebar on escape key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && sidebar.classList.contains('show')) {
                sidebar.classList.remove('show');
            }
        });
    }

    // ==========================================
    // AJAX HELPERS
    // ==========================================
    
    // ==========================================
    // DARK MODE TOGGLE
    // ==========================================
    
    function initDarkMode() {
        const darkModeToggle = document.getElementById('darkModeToggle');
        const sidebarDarkModeToggle = document.getElementById('sidebarDarkModeToggle');
        
        // Get initial theme from localStorage or default to 'light'
        const theme = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-theme', theme);
        
        // Toggle theme function
        function toggleTheme() {
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            
            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            
            // Add smooth transition class temporarily
            document.documentElement.classList.add('theme-transition');
            setTimeout(() => {
                document.documentElement.classList.remove('theme-transition');
            }, 300);
            
            // Log for debugging
            console.log('Theme changed to:', newTheme);
        }
        
        // Header toggle button
        if (darkModeToggle) {
            darkModeToggle.addEventListener('click', toggleTheme);
        }
        
        // Sidebar toggle button
        if (sidebarDarkModeToggle) {
            sidebarDarkModeToggle.addEventListener('click', function(e) {
                e.preventDefault();
                toggleTheme();
            });
        }
    }

    // ==========================================
    // AJAX HELPERS
    // ==========================================
    
    window.AMS = window.AMS || {};
    
    /**
     * Make an AJAX request
     */
    AMS.ajax = function(options) {
        const defaults = {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };
        
        const config = { ...defaults, ...options };
        
        // Add anti-forgery token for POST/PUT/DELETE
        if (['POST', 'PUT', 'DELETE'].includes(config.method.toUpperCase())) {
            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                if (config.headers['Content-Type'] === 'application/json') {
                    config.headers['RequestVerificationToken'] = token.value;
                }
            }
        }
        
        return fetch(config.url, config)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    return response.json();
                }
                return response.text();
            });
    };
    
    /**
     * Submit form via AJAX
     */
    AMS.submitForm = function(form, options = {}) {
        const formData = new FormData(form);
        const url = options.url || form.action;
        const method = options.method || form.method || 'POST';
        
        const config = {
            method: method,
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        };
        
        return fetch(url, config)
            .then(response => {
                if (!response.ok) {
                    return response.json().then(err => Promise.reject(err));
                }
                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    return response.json();
                }
                return response.text();
            });
    };

    // ==========================================
    // SWEETALERT2 WRAPPERS
    // ==========================================
    
    /**
     * Show success message
     */
    AMS.success = function(title, text) {
        if (typeof Swal === 'undefined') {
            alert(title + (text ? '\n' + text : ''));
            return Promise.resolve();
        }
        return Swal.fire({
            icon: 'success',
            title: title,
            text: text,
            timer: 2000,
            showConfirmButton: false
        });
    };
    
    /**
     * Show error message
     */
    AMS.error = function(title, text) {
        if (typeof Swal === 'undefined') {
            alert(title + (text ? '\n' + text : ''));
            return Promise.resolve();
        }
        return Swal.fire({
            icon: 'error',
            title: title,
            text: text
        });
    };
    
    /**
     * Show warning message
     */
    AMS.warning = function(title, text) {
        if (typeof Swal === 'undefined') {
            alert(title + (text ? '\n' + text : ''));
            return Promise.resolve();
        }
        return Swal.fire({
            icon: 'warning',
            title: title,
            text: text
        });
    };
    
    /**
     * Show confirmation dialog
     */
    AMS.confirm = function(options) {
        const defaults = {
            title: 'Are you sure?',
            text: 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#4f46e5',
            cancelButtonColor: '#64748b',
            confirmButtonText: 'Yes, proceed',
            cancelButtonText: 'Cancel'
        };
        
        if (typeof Swal === 'undefined') {
            return Promise.resolve({ isConfirmed: confirm(options.title || defaults.title) });
        }
        
        return Swal.fire({ ...defaults, ...options });
    };
    
    /**
     * Show delete confirmation
     */
    AMS.confirmDelete = function(itemName) {
        return AMS.confirm({
            title: 'Delete ' + (itemName || 'Item') + '?',
            text: 'This action cannot be undone.',
            icon: 'warning',
            confirmButtonText: 'Yes, delete it',
            confirmButtonColor: '#ef4444'
        });
    };
    
    /**
     * Show toast notification
     */
    AMS.toast = function(message, type = 'success') {
        if (typeof Swal === 'undefined') {
            console.log(message);
            return;
        }
        
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });
        
        Toast.fire({
            icon: type,
            title: message
        });
    };

    // ==========================================
    // FORM HANDLING
    // ==========================================
    
    /**
     * Handle AJAX form submission
     */
    AMS.handleAjaxForm = function(formSelector, options = {}) {
        const forms = document.querySelectorAll(formSelector);
        
        forms.forEach(form => {
            form.addEventListener('submit', async function(e) {
                e.preventDefault();
                
                const submitBtn = form.querySelector('[type="submit"]');
                const originalText = submitBtn ? submitBtn.innerHTML : '';
                
                // Show loading state
                if (submitBtn) {
                    submitBtn.disabled = true;
                    submitBtn.classList.add('btn-loading');
                }
                
                try {
                    const response = await AMS.submitForm(form, options);
                    
                    if (options.onSuccess) {
                        options.onSuccess(response, form);
                    } else {
                        AMS.success('Success', 'Operation completed successfully');
                        if (options.redirectUrl) {
                            window.location.href = options.redirectUrl;
                        } else if (options.reloadPage) {
                            window.location.reload();
                        }
                    }
                } catch (error) {
                    if (options.onError) {
                        options.onError(error, form);
                    } else {
                        let errorMessage = 'An error occurred. Please try again.';
                        if (error.errors) {
                            errorMessage = Object.values(error.errors).flat().join('\n');
                        } else if (error.message) {
                            errorMessage = error.message;
                        }
                        AMS.error('Error', errorMessage);
                    }
                } finally {
                    // Reset button state
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.classList.remove('btn-loading');
                        submitBtn.innerHTML = originalText;
                    }
                }
            });
        });
    };
    
    /**
     * Handle delete buttons
     */
    AMS.handleDeleteButtons = function(selector, options = {}) {
        document.addEventListener('click', async function(e) {
            const btn = e.target.closest(selector);
            if (!btn) return;
            
            e.preventDefault();
            
            const url = btn.dataset.url || btn.href;
            const itemName = btn.dataset.name || 'item';
            
            const result = await AMS.confirmDelete(itemName);
            
            if (result.isConfirmed) {
                try {
                    const response = await AMS.ajax({
                        url: url,
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });
                    
                    AMS.success('Deleted', `${itemName} has been deleted.`);
                    
                    if (options.onSuccess) {
                        options.onSuccess(response);
                    } else if (options.removeElement) {
                        const row = btn.closest(options.removeElement);
                        if (row) {
                            row.remove();
                        }
                    } else {
                        window.location.reload();
                    }
                } catch (error) {
                    AMS.error('Error', 'Failed to delete. Please try again.');
                }
            }
        });
    };

    // ==========================================
    // MODAL HELPERS
    // ==========================================
    
    /**
     * Open modal with content from URL
     */
    AMS.openModal = function(modalId, url) {
        const modal = document.getElementById(modalId);
        if (!modal) return;
        
        const modalBody = modal.querySelector('.modal-body');
        const bsModal = new bootstrap.Modal(modal);
        
        // Show loading
        modalBody.innerHTML = '<div class="text-center py-4"><div class="loading-spinner mx-auto"></div></div>';
        bsModal.show();
        
        // Load content
        fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
        .then(response => response.text())
        .then(html => {
            modalBody.innerHTML = html;
            // Re-initialize validation if needed
            if ($.validator && $.validator.unobtrusive) {
                $.validator.unobtrusive.parse(modalBody);
            }
        })
        .catch(error => {
            modalBody.innerHTML = '<div class="alert alert-danger">Failed to load content</div>';
        });
    };
    
    /**
     * Close modal
     */
    AMS.closeModal = function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
        }
    };

    // ==========================================
    // TABLE HELPERS
    // ==========================================
    
    /**
     * Initialize sortable table
     */
    AMS.initSortableTable = function(tableSelector) {
        const table = document.querySelector(tableSelector);
        if (!table) return;
        
        const headers = table.querySelectorAll('th[data-sort]');
        
        headers.forEach(header => {
            header.style.cursor = 'pointer';
            header.addEventListener('click', function() {
                const column = this.dataset.sort;
                const tbody = table.querySelector('tbody');
                const rows = Array.from(tbody.querySelectorAll('tr'));
                const isAsc = !this.classList.contains('sort-asc');
                
                // Remove sort classes from all headers
                headers.forEach(h => h.classList.remove('sort-asc', 'sort-desc'));
                
                // Add sort class to current header
                this.classList.add(isAsc ? 'sort-asc' : 'sort-desc');
                
                // Sort rows
                rows.sort((a, b) => {
                    const aVal = a.querySelector(`td:nth-child(${this.cellIndex + 1})`).textContent.trim();
                    const bVal = b.querySelector(`td:nth-child(${this.cellIndex + 1})`).textContent.trim();
                    
                    const aNum = parseFloat(aVal);
                    const bNum = parseFloat(bVal);
                    
                    if (!isNaN(aNum) && !isNaN(bNum)) {
                        return isAsc ? aNum - bNum : bNum - aNum;
                    }
                    
                    return isAsc ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
                });
                
                // Re-append sorted rows
                rows.forEach(row => tbody.appendChild(row));
            });
        });
    };
    
    /**
     * Initialize table search
     */
    AMS.initTableSearch = function(inputSelector, tableSelector) {
        const input = document.querySelector(inputSelector);
        const table = document.querySelector(tableSelector);
        
        if (!input || !table) return;
        
        input.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            const rows = table.querySelectorAll('tbody tr');
            
            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(searchTerm) ? '' : 'none';
            });
        });
    };

    // ==========================================
    // UTILITY FUNCTIONS
    // ==========================================
    
    /**
     * Format currency
     */
    AMS.formatCurrency = function(amount, currency = 'USD') {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: currency
        }).format(amount);
    };
    
    /**
     * Format date
     */
    AMS.formatDate = function(date, format = 'short') {
        const d = new Date(date);
        const options = format === 'long' 
            ? { year: 'numeric', month: 'long', day: 'numeric' }
            : { year: 'numeric', month: '2-digit', day: '2-digit' };
        return d.toLocaleDateString('en-US', options);
    };
    
    /**
     * Format number with commas
     */
    AMS.formatNumber = function(num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    };
    
    /**
     * Debounce function
     */
    AMS.debounce = function(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    };
    
    /**
     * Copy to clipboard
     */
    AMS.copyToClipboard = function(text) {
        navigator.clipboard.writeText(text).then(() => {
            AMS.toast('Copied to clipboard');
        }).catch(() => {
            AMS.error('Error', 'Failed to copy');
        });
    };

    // ==========================================
    // GENERIC NO-REFRESH CRUD SYSTEM
    // ==========================================
    
    AMS.CRUD = {
        config: {},
        
        /**
         * Initialize CRUD handler
         * @param {Object} config Configuration object with entity details
         */
        init: function(config) {
            this.config = config;
            this.attachEventHandlers();
        },
        
        /**
         * Attach event handlers for CRUD operations
         */
        attachEventHandlers: function() {
            const self = this;
            
            // Create button
            $(document).on('click', '.btn-create', function(e) {
                e.preventDefault();
                self.openCreateModal();
            });
            
            // Edit button
            $(document).on('click', '.btn-edit', function(e) {
                e.preventDefault();
                const id = $(this).data('id');
                self.openEditModal(id);
            });
            
            // Delete button
            $(document).on('click', '.btn-delete', function(e) {
                e.preventDefault();
                const id = $(this).data('id');
                const name = $(this).data('name');
                self.deleteRecord(id, name);
            });
            
            // Form submission (Create/Edit)
            $(document).on('submit', `#create${self.capitalize(self.config.entity)}Form, #edit${self.capitalize(self.config.entity)}Form`, function(e) {
                e.preventDefault();
                self.submitForm($(this));
            });
        },
        
        /**
         * Open create modal
         */
        openCreateModal: function() {
            const self = this;
            const modal = $(`#${self.config.modalId}`);
            const modalBody = modal.find('.modal-body');
            const modalTitle = modal.find('.modal-title');
            
            modalTitle.text(`Add New ${self.capitalize(self.config.entity)}`);
            modalBody.html('<div class="text-center py-4"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
            
            modal.modal('show');
            
            $.ajax({
                url: self.config.createUrl,
                type: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function(response) {
                    modalBody.html(response);
                    // Reinitialize validation
                    if ($.validator && $.validator.unobtrusive) {
                        $.validator.unobtrusive.parse(modalBody);
                    }
                },
                error: function() {
                    modalBody.html('<div class="alert alert-danger">Failed to load form. Please try again.</div>');
                }
            });
        },
        
        /**
         * Open edit modal
         */
        openEditModal: function(id) {
            const self = this;
            const modal = $(`#${self.config.modalId}`);
            const modalBody = modal.find('.modal-body');
            const modalTitle = modal.find('.modal-title');
            
            modalTitle.text(`Edit ${self.capitalize(self.config.entity)}`);
            modalBody.html('<div class="text-center py-4"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
            
            modal.modal('show');
            
            $.ajax({
                url: `${self.config.editUrl}/${id}`,
                type: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function(response) {
                    modalBody.html(response);
                    // Reinitialize validation
                    if ($.validator && $.validator.unobtrusive) {
                        $.validator.unobtrusive.parse(modalBody);
                    }
                },
                error: function() {
                    modalBody.html('<div class="alert alert-danger">Failed to load form. Please try again.</div>');
                }
            });
        },
        
        /**
         * Submit form (Create or Edit)
         */
        submitForm: function($form) {
            const self = this;
            
            // Validate form
            if ($form.valid && !$form.valid()) {
                return false;
            }
            
            const submitBtn = $form.find('button[type="submit"]');
            const originalText = submitBtn.html();
            
            // Disable button and show loading
            submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Saving...');
            
            $.ajax({
                url: $form.attr('action'),
                type: 'POST',
                data: $form.serialize(),
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function(response) {
                    if (response.success) {
                        // Close modal
                        $(`#${self.config.modalId}`).modal('hide');
                        
                        // Show success message
                        AMS.toast(response.message, 'success');
                        
                        // Reload table data
                        self.reloadTable();
                    } else {
                        // Show validation errors
                        if (response.errors) {
                            if (Array.isArray(response.errors)) {
                                let errorHtml = '<div class="alert alert-danger"><ul class="mb-0">';
                                response.errors.forEach(function(error) {
                                    errorHtml += `<li>${error}</li>`;
                                });
                                errorHtml += '</ul></div>';
                                $form.prepend(errorHtml);
                            } else {
                                // Display field-specific errors
                                Object.keys(response.errors).forEach(function(field) {
                                    const input = $form.find(`[name="${field}"]`);
                                    if (input.length) {
                                        const errorSpan = input.siblings('.text-danger');
                                        if (errorSpan.length) {
                                            errorSpan.text(response.errors[field][0] || response.errors[field]);
                                        } else {
                                            input.after(`<span class="text-danger">${response.errors[field][0] || response.errors[field]}</span>`);
                                        }
                                        input.addClass('is-invalid');
                                    }
                                });
                            }
                        }
                    }
                },
                error: function(xhr) {
                    let errorMessage = 'An error occurred. Please try again.';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    AMS.error('Error', errorMessage);
                },
                complete: function() {
                    submitBtn.prop('disabled', false).html(originalText);
                }
            });
            
            return false;
        },
        
        /**
         * Delete record
         */
        deleteRecord: function(id, name) {
            const self = this;
            
            Swal.fire({
                title: `Delete ${self.capitalize(self.config.entity)}?`,
                html: `Are you sure you want to delete <strong>${name || 'this record'}</strong>?<br><small class="text-danger">This action cannot be undone.</small>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ef4444',
                cancelButtonColor: '#64748b',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel',
                showLoaderOnConfirm: true,
                preConfirm: () => {
                    return $.ajax({
                        url: self.config.deleteUrl,
                        type: 'POST',
                        data: {
                            id: id,
                            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                        },
                        headers: { 'X-Requested-With': 'XMLHttpRequest' }
                    }).then(response => {
                        return response;
                    }).catch(error => {
                        Swal.showValidationMessage(`Request failed: ${error.statusText}`);
                    });
                },
                allowOutsideClick: () => !Swal.isLoading()
            }).then((result) => {
                if (result.isConfirmed && result.value) {
                    if (result.value.success) {
                        AMS.toast(result.value.message, 'success');
                        self.reloadTable();
                    } else {
                        AMS.error('Error', result.value.message || 'Failed to delete record.');
                    }
                }
            });
        },
        
        /**
         * Reload table data
         */
        reloadTable: function() {
            const self = this;
            const tableBody = $(`#${self.config.tableBodyId}`);
            
            // Show loading state
            tableBody.html('<tr><td colspan="100%" class="text-center py-4"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></td></tr>');
            
            $.ajax({
                url: self.config.getTableDataUrl,
                type: 'GET',
                success: function(response) {
                    tableBody.html(response);
                    
                    // Update count if there's a counter element
                    const rowCount = tableBody.find('tr:not(#emptyRow)').length;
                    $('#totalCount').text(rowCount);
                },
                error: function() {
                    tableBody.html('<tr><td colspan="100%" class="text-center py-4"><div class="alert alert-danger mb-0">Failed to load data. Please refresh the page.</div></td></tr>');
                }
            });
        },
        
        /**
         * Capitalize first letter
         */
        capitalize: function(str) {
            return str.charAt(0).toUpperCase() + str.slice(1);
        }
    };

    // ==========================================
    // FORM VALIDATION HELPERS
    // ==========================================
    
    /**
     * Show validation errors on form
     */
    AMS.showValidationErrors = function(form, errors) {
        // Clear previous errors
        form.querySelectorAll('.is-invalid').forEach(el => {
            el.classList.remove('is-invalid');
        });
        form.querySelectorAll('.invalid-feedback').forEach(el => {
            el.remove();
        });
        
        // Show new errors
        Object.keys(errors).forEach(field => {
            const input = form.querySelector(`[name="${field}"]`);
            if (input) {
                input.classList.add('is-invalid');
                const feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = Array.isArray(errors[field]) ? errors[field][0] : errors[field];
                input.parentNode.appendChild(feedback);
            }
        });
    };
    
    /**
     * Clear validation errors
     */
    AMS.clearValidationErrors = function(form) {
        form.querySelectorAll('.is-invalid').forEach(el => {
            el.classList.remove('is-invalid');
        });
        form.querySelectorAll('.invalid-feedback').forEach(el => {
            el.remove();
        });
    };

    // ==========================================
    // LOADING STATES
    // ==========================================
    
    /**
     * Show page loading overlay
     */
    AMS.showLoading = function() {
        let overlay = document.querySelector('.page-loading-overlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.className = 'page-loading-overlay';
            overlay.innerHTML = '<div class="loading-spinner"></div>';
            overlay.style.cssText = 'position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(255,255,255,0.8);display:flex;align-items:center;justify-content:center;z-index:9999';
            document.body.appendChild(overlay);
        }
        overlay.style.display = 'flex';
    };
    
    /**
     * Hide page loading overlay
     */
    AMS.hideLoading = function() {
        const overlay = document.querySelector('.page-loading-overlay');
        if (overlay) {
            overlay.style.display = 'none';
        }
    };

    // ==========================================
    // TOGGLE SWITCH HELPER
    // ==========================================
    
    function initToggleSwitches() {
        // Find all toggle switches
        const toggleSwitches = document.querySelectorAll('.form-switch .form-check-input[type="checkbox"]');
        
        toggleSwitches.forEach(toggleInput => {
            // Ensure the input has a proper ID for label association
            if (!toggleInput.id) {
                toggleInput.id = toggleInput.name || 'toggle-' + Math.random().toString(36).substr(2, 9);
            }
            
            // Get associated label
            const label = toggleInput.closest('.form-switch')?.querySelector('label[for="' + toggleInput.id + '"]');
            
            // Click handler for the input itself
            toggleInput.addEventListener('click', function(e) {
                // Let the default behavior happen
                // The checkbox will toggle automatically
                
                // Force a visual update after a tiny delay
                setTimeout(() => {
                    if (this.checked) {
                        this.setAttribute('checked', 'checked');
                    } else {
                        this.removeAttribute('checked');
                    }
                }, 10);
            });
            
            // Click handler for the label
            if (label) {
                label.addEventListener('click', function(e) {
                    // Prevent double-toggle (label click already toggles the input)
                    // We just need to ensure visual sync
                    setTimeout(() => {
                        if (toggleInput.checked) {
                            toggleInput.setAttribute('checked', 'checked');
                        } else {
                            toggleInput.removeAttribute('checked');
                        }
                    }, 10);
                });
            }
            
            // Change event for additional reliability
            toggleInput.addEventListener('change', function(e) {
                console.log('Toggle changed:', this.id, 'Checked:', this.checked);
                
                // Ensure the visual state matches
                if (this.checked) {
                    this.setAttribute('checked', 'checked');
                } else {
                    this.removeAttribute('checked');
                }
            });
            
            // Initialize the visual state
            if (toggleInput.checked) {
                toggleInput.setAttribute('checked', 'checked');
            }
        });
    }
    
    // ==========================================
    // FORM SUBMISSION HELPER FOR TOGGLE SWITCHES
    // ==========================================
    
    function ensureToggleValuesBeforeSubmit() {
        // Listen for form submissions
        document.addEventListener('submit', function(e) {
            const form = e.target;
            
            // Find all toggle switches in this form
            const toggles = form.querySelectorAll('.form-switch .form-check-input[type="checkbox"]');
            
            toggles.forEach(toggle => {
                // Log the current state for debugging
                console.log('Form submitting - Toggle:', toggle.id || toggle.name, 'Value:', toggle.checked);
                
                // Ensure the value is properly set
                // For ASP.NET Core model binding, checkboxes need to be properly bound
                if (!toggle.checked) {
                    // Create a hidden input to ensure false values are posted
                    const hiddenInput = form.querySelector('input[name="' + toggle.name + '"][type="hidden"]');
                    if (hiddenInput) {
                        hiddenInput.value = 'false';
                    }
                }
            });
        }, true); // Use capture phase to run before form submission
    }

    // ==========================================
    // INITIALIZATION
    // ==========================================
    
    function init() {
        initSidebar();
        initDarkMode();
        initToggleSwitches();
        ensureToggleValuesBeforeSubmit();
        
        // Initialize tooltips
        const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        tooltips.forEach(el => new bootstrap.Tooltip(el));
        
        // Initialize popovers
        const popovers = document.querySelectorAll('[data-bs-toggle="popover"]');
        popovers.forEach(el => new bootstrap.Popover(el));
        
        // Auto-hide alerts after 5 seconds
        document.querySelectorAll('.alert-dismissible').forEach(alert => {
            setTimeout(() => {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
                bsAlert.close();
            }, 5000);
        });
        
        // Handle logout confirmation
        const logoutBtn = document.querySelector('.logout-btn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', function(e) {
                e.preventDefault();
                AMS.confirm({
                    title: 'Logout',
                    text: 'Are you sure you want to logout?',
                    icon: 'question',
                    confirmButtonText: 'Yes, logout'
                }).then(result => {
                    if (result.isConfirmed) {
                        document.getElementById('logoutForm').submit();
                    }
                });
            });
        }
    }
    
    // Run on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
