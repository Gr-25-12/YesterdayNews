// newsChat.js - News Chat Interface
$(document).ready(function() {
    const NewsChat = {
        isOpen: false,
        conversationHistory: [],
        
        // DOM Elements
        elements: {
            toggle: $('#newsChatToggle'),
            container: $('#newsChatContainer'),
            close: $('#newsChatClose'),
            messages: $('#newsChatMessages'),
            input: $('#newsChatInput'),
            send: $('#newsChatSend'),
            typing: $('#newsChatTyping')
        },

        // Initialize chat
        init: function() {
            this.bindEvents();
            this.adjustInputHeight();
        },

        // Bind event listeners
        bindEvents: function() {
            const self = this;

            // Toggle chat
            this.elements.toggle.on('click', function() {
                self.toggleChat();
            });

            // Close chat
            this.elements.close.on('click', function() {
                self.closeChat();
            });

            // Send message on button click
            this.elements.send.on('click', function() {
                self.sendMessage();
            });

            // Send message on Enter key (but allow Shift+Enter for new line)
            this.elements.input.on('keypress', function(e) {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    self.sendMessage();
                }
            });

            // Auto-resize textarea and enable/disable send button
            this.elements.input.on('input', function() {
                self.adjustInputHeight();
                self.toggleSendButton();
            });

            // Close chat when clicking outside
            $(document).on('click', function(e) {
                if (self.isOpen && 
                    !self.elements.container.is(e.target) && 
                    self.elements.container.has(e.target).length === 0 &&
                    !self.elements.toggle.is(e.target) && 
                    self.elements.toggle.has(e.target).length === 0) {
                    self.closeChat();
                }
            });

            // Prevent chat container clicks from bubbling
            this.elements.container.on('click', function(e) {
                e.stopPropagation();
            });
        },

        // Toggle chat visibility
        toggleChat: function() {
            if (this.isOpen) {
                this.closeChat();
            } else {
                this.openChat();
            }
        },

        // Open chat
        openChat: function() {
            this.isOpen = true;
            this.elements.container.addClass('show');
            this.elements.input.focus();
            this.scrollToBottom();
        },

        // Close chat
        closeChat: function() {
            this.isOpen = false;
            this.elements.container.removeClass('show');
        },

        // Send message
        sendMessage: function() {
            const message = this.elements.input.val().trim();
            if (!message) return;

            // Add user message to chat
            this.addMessage('user', message);
            
            // Clear input and disable send button
            this.elements.input.val('');
            this.adjustInputHeight();
            this.toggleSendButton();

            // Store in conversation history
            this.conversationHistory.push({
                type: 'user',
                content: message,
                timestamp: new Date().toISOString()
            });

            // Show typing indicator
            this.showTyping();

            // Send to server
            this.sendToServer(message);
        },

        // Add message to chat
        addMessage: function(type, content) {
            let processedContent;
            
            if (type === 'bot') {
                // Convert Markdown formatting to HTML for bot messages
                processedContent = this.markdownToHtml(content);
            } else {
                // Just escape HTML for user messages
                processedContent = this.escapeHtml(content);
            }
            
            const messageHtml = `
                <div class="news-chat-message ${type}">
                    ${processedContent}
                </div>
            `;
            
            this.elements.messages.append(messageHtml);
            this.scrollToBottom();
        },

        // Show typing indicator
        showTyping: function() {
            this.elements.typing.show();
            this.scrollToBottom();
        },

        // Hide typing indicator
        hideTyping: function() {
            this.elements.typing.hide();
        },

        // Send message to server
        sendToServer: function(message) {
            const self = this;
            
            $.ajax({
                url: '/Chat/AskNews',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    message: message,
                    conversationHistory: this.conversationHistory.slice(-10) // Send last 10 messages
                }),
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function(response) {
                    self.hideTyping();
                    
                    if (response.success && response.response) {
                        self.addMessage('bot', response.response);
                        
                        // Store bot response in history
                        self.conversationHistory.push({
                            type: 'bot',
                            content: response.response,
                            timestamp: new Date().toISOString()
                        });
                    } else {
                        self.addMessage('bot', response.error || 'Sorry, I encountered an error. Please try again.');
                    }
                },
                error: function(xhr, status, error) {
                    self.hideTyping();
                    console.error('Chat error:', error);
                    
                    let errorMessage = 'Sorry, I encountered an error. Please try again.';
                    
                    if (xhr.status === 429) {
                        errorMessage = 'Too many requests. Please wait a moment and try again.';
                    } else if (xhr.status === 0) {
                        errorMessage = 'Connection error. Please check your internet connection.';
                    }
                    
                    self.addMessage('bot', errorMessage);
                }
            });
        },

        // Adjust textarea height based on content
        adjustInputHeight: function() {
            const textarea = this.elements.input[0];
            textarea.style.height = 'auto';
            textarea.style.height = Math.min(textarea.scrollHeight, 100) + 'px';
        },

        // Enable/disable send button based on input content
        toggleSendButton: function() {
            const hasContent = this.elements.input.val().trim().length > 0;
            this.elements.send.prop('disabled', !hasContent);
        },

        // Scroll messages to bottom
        scrollToBottom: function() {
            this.elements.messages.scrollTop(this.elements.messages[0].scrollHeight);
        },

        // Convert basic Markdown to HTML
        markdownToHtml: function(text) {
            // First escape HTML to prevent XSS
            let html = this.escapeHtml(text);
            
            // Convert Markdown formatting
            html = html
                // Bold: **text** or __text__
                .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
                .replace(/__(.*?)__/g, '<strong>$1</strong>')
                // Italic: *text* or _text_
                .replace(/\*(.*?)\*/g, '<em>$1</em>')
                .replace(/_(.*?)_/g, '<em>$1</em>')
                // Code: `text`
                .replace(/`(.*?)`/g, '<code style="background: #f1f3f4; padding: 2px 4px; border-radius: 3px; font-family: monospace;">$1</code>')
                // Line breaks
                .replace(/\n/g, '<br>');
            
            return html;
        },

        // Escape HTML to prevent XSS
        escapeHtml: function(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
    };

    // Initialize chat
    NewsChat.init();

    // Make NewsChat available globally for debugging
    window.NewsChat = NewsChat;
});