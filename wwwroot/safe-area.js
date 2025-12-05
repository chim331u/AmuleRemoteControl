// Safe Area JavaScript Helper
// Detects status bar height and applies padding to Radzen components

(function() {
    'use strict';

    function applySafeArea() {
        // Get the actual viewport height vs window height to detect status bar
        const windowHeight = window.innerHeight;
        const screenHeight = window.screen.height;

        // Calculate status bar height
        // On Android, the difference between screen.height and innerHeight includes status bar
        let statusBarHeight = 0;

        // Detect if we're in a WebView (MAUI environment)
        const isWebView = window.navigator.userAgent.includes('wv') ||
                          window.webkit?.messageHandlers !== undefined;

        if (isWebView) {
            // In MAUI WebView, we need to account for status bar
            // Use a reasonable default for Android status bars (24-48dp)
            const density = window.devicePixelRatio || 1;
            statusBarHeight = 24 * density; // 24dp is standard Android status bar

            // For devices with larger status bars (notches, etc.)
            const heightDifference = screenHeight - windowHeight;
            if (heightDifference > statusBarHeight) {
                statusBarHeight = Math.min(heightDifference, 60 * density);
            }
        }

        // Apply padding to Radzen header
        const headers = document.querySelectorAll('.rz-header');
        headers.forEach(header => {
            if (statusBarHeight > 0) {
                const currentPadding = parseInt(getComputedStyle(header).paddingTop) || 0;
                const newPadding = Math.max(currentPadding, statusBarHeight);
                header.style.paddingTop = newPadding + 'px';
                header.style.transition = 'padding-top 0.3s ease';
            }
        });

        // Apply padding to Radzen footer (for home indicators on iOS-style Android)
        const footers = document.querySelectorAll('.rz-footer');
        footers.forEach(footer => {
            // Some Android devices have gesture navigation with home indicators
            const bottomInset = Math.max(0, window.innerHeight - document.documentElement.clientHeight);
            if (bottomInset > 0) {
                footer.style.paddingBottom = bottomInset + 'px';
            }
        });

        console.log('[SafeArea] Applied status bar padding:', statusBarHeight + 'px');
    }

    // Apply on load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', applySafeArea);
    } else {
        applySafeArea();
    }

    // Re-apply on orientation change
    window.addEventListener('orientationchange', function() {
        setTimeout(applySafeArea, 100);
    });

    // Re-apply when Blazor finishes rendering (for SPA navigation)
    if (window.Blazor) {
        window.Blazor.addEventListener('enhancedload', applySafeArea);
    }

    // Fallback: Check periodically for the first 3 seconds (in case DOM isn't ready)
    let checks = 0;
    const checkInterval = setInterval(function() {
        checks++;
        if (checks >= 10 || document.querySelector('.rz-header')) {
            applySafeArea();
            if (checks >= 10) clearInterval(checkInterval);
        }
    }, 300);

    // Expose function globally for manual calls
    window.applySafeArea = applySafeArea;
})();
