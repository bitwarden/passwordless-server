import { createApp } from 'vue';
import Modal from './modal.mjs';

document.addEventListener('DOMContentLoaded', function () {
    
    // create the modal container
    const modalEl = document.createElement('div');
    modalEl.id = 'modal-container';
    document.body.appendChild(modalEl);
        
    // intercept forms
    document.querySelectorAll('[confirm-submit]').forEach(function (element) {
        
        element.addEventListener('submit', function (event) {
            event.preventDefault();
            const form = event.target;

            // Not sure, but I think we need to run validate.
            form.checkValidity();

            var modal = createApp(Modal, {
                form: form,
                title: form.getAttribute('confirm-submit'),
                active: true
            }).mount(modalEl);
            return false;
        });
    });
});