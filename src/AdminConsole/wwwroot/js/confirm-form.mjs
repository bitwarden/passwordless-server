import { createApp } from 'vue';
import Modal, { showModal } from './modal.mjs';

document.addEventListener('DOMContentLoaded', function () {
    
    // create the modal container
    const modalEl = document.createElement('div');
    modalEl.id = 'modal-container';
    document.body.appendChild(modalEl);

    var modal = createApp(Modal).mount(modalEl);
        
    // intercept forms
    document.querySelectorAll('[confirm-submit]').forEach(function (element) {        
        element.addEventListener('submit', function (event) {
            event.preventDefault();
            const form = event.target;                       
                                  
            // Not sure, but I think we need to run validate.
            form.checkValidity();
            
            // show the modal and set copy            
            showModal({
                show: true,
                form,
                submitter: { name: event.submitter?.getAttribute("name"), value: event.submitter?.getAttribute("value")},
                title: event.submitter.getAttribute('confirm-title') || 'Are you really sure?'
            });            
            
            
            return false;
        });
    });
});