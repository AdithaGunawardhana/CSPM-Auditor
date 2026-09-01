class NexusNavbar extends HTMLElement {
    connectedCallback() {
        this.innerHTML = `
        <div id="main-nav-wrapper" class="fixed top-6 left-0 w-full z-[100] flex justify-center transition-all duration-500 ease-in-out transform">
            <div class="w-[95%] max-w-[1200px]">
                <nav class="bg-zinc-900/80 backdrop-blur-xl border border-zinc-800 rounded-full px-6 h-16 flex items-center justify-between shadow-2xl">
                    
                    <a href="index.html" class="flex items-center gap-3 group">
                        <div class="w-8 h-8 rounded-full bg-gradient-to-tr from-amber-500 to-rose-500 flex items-center justify-center shadow-inner group-hover:scale-105 transition-transform">
                            <div class="w-3 h-3 bg-zinc-950 rounded-sm rotate-45"></div>
                        </div>
                        <span class="text-white font-bold tracking-wide text-lg">Nexus</span>
                    </a>

                    <div class="hidden md:flex items-center gap-8 text-sm font-medium text-zinc-400">
                        <a href="platform.html" class="hover:text-amber-500 transition-colors">Platform</a>
                        <a href="solutions.html" class="hover:text-amber-500 transition-colors">Solutions</a>
                        <a href="documentation.html" class="hover:text-amber-500 transition-colors">Documentation</a>
                    </div>

                    <div class="flex items-center gap-4">
                        <div class="relative">
                            <button id="profileBtn" class="flex items-center gap-2 pl-2 pr-1 py-1 rounded-full bg-zinc-800 hover:bg-zinc-700 border border-zinc-700 transition-all focus:outline-none focus:ring-2 focus:ring-amber-500/50 cursor-pointer">
                                <span id="nav-user-name" class="text-xs font-semibold text-zinc-300 hidden sm:block">Guest User</span>
                                <div id="nav-avatar-container" class="w-8 h-8 rounded-full bg-zinc-900 flex items-center justify-center border border-zinc-600 overflow-hidden">
                                    <svg id="nav-default-icon" class="w-4 h-4 text-zinc-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                                    <img id="nav-profile-pic" src="" alt="Profile" referrerpolicy="no-referrer" class="w-full h-full object-cover hidden">
                                </div>
                            </button>
                            
                            <div id="dropdownMenu" class="absolute right-0 mt-4 w-64 bg-zinc-900/95 backdrop-blur-xl border border-zinc-700/50 rounded-2xl shadow-[0_10px_40px_rgba(0,0,0,0.8)] p-2 z-[999] opacity-0 invisible scale-95 translate-y-2 origin-top-right transition-all duration-200">
    
                                <!-- 1. THE LOGGED OUT MENU (Shows only when logged out) -->
                                <div id="menu-logged-out" class="py-1 block">
                                    <div class="px-3 py-2 mb-1"><p class="text-xs text-zinc-400">Sign in to access Nexus</p></div>
                                    <a href="login.html" class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-zinc-300 hover:bg-white hover:text-zinc-900 transition-all cursor-pointer">
                                        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1"></path></svg> Log In
                                    </a>
                                </div>

                                <!-- 2. THE LOGGED IN MENU (Shows only when logged in) -->
                                <div id="menu-logged-in" class="hidden">
                                    <div class="px-3 py-3 border-b border-zinc-800 mb-2">
                                        <p id="dropdown-name" class="text-sm font-bold text-white">Loading...</p>
                                        <p id="dropdown-email" class="text-xs text-zinc-400 mt-0.5 truncate">loading...</p>
                                    </div>
                                    <div class="py-1 space-y-1">
                                        <a href="dashboard.html" class="flex items-center gap-3 px-3 py-2 rounded-xl text-xs font-medium text-zinc-300 hover:bg-amber-500/10 hover:text-amber-500 transition-all">
                                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012-2h-2a2 2 0 01-2-2v-2z"></path></svg> CSPM Dashboard
                                        </a>
                                        <a href="profile.html" class="flex items-center gap-3 px-3 py-2 rounded-xl text-xs font-medium text-zinc-300 hover:bg-sky-500/10 hover:text-sky-400 transition-all">
                                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg> User Profile
                                        </a>
                                        <a href="apply-admin.html" class="flex items-center gap-3 px-3 py-2 rounded-xl text-xs font-medium text-indigo-300 hover:bg-indigo-500/10 hover:text-indigo-400 transition-all">
                                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path></svg> Apply for an Admin
                                        </a>
                                        <a href="settings.html" class="flex items-center gap-3 px-3 py-2 rounded-xl text-xs font-medium text-emerald-300 hover:bg-emerald-500/10 hover:text-emerald-400 transition-all">
                                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                                            </svg> 
                                            Preferences
                                        </a>
                                        
                                        <!-- RESTORED THE PROPER LOGOUT BUTTON HERE -->
                                        <div class="border-t border-zinc-800/60 mt-2 pt-2">
                                            <button id="navLogoutBtn" class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-medium text-rose-400 hover:bg-rose-500/10 transition-all text-left cursor-pointer">
                                                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path></svg> Log Out
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </nav>
            </div>
        </div>
        `;

        setTimeout(() => this.initLogic(), 10);
    }

    initLogic() {
        const profileBtn = this.querySelector('#profileBtn');
        const dropdownMenu = this.querySelector('#dropdownMenu');
        const mainNavWrapper = this.querySelector('#main-nav-wrapper'); // Grab the wrapper for scrolling

        if(profileBtn && dropdownMenu) {
            profileBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                dropdownMenu.classList.toggle('opacity-0');
                dropdownMenu.classList.toggle('invisible');
                dropdownMenu.classList.toggle('scale-95');
                dropdownMenu.classList.toggle('translate-y-2');
            });

            document.addEventListener('click', (e) => {
                if (!dropdownMenu.contains(e.target) && !profileBtn.contains(e.target)) {
                    dropdownMenu.classList.add('opacity-0', 'invisible', 'scale-95', 'translate-y-2');
                }
            });
        }

        // ==========================================
        // NEW: AUTO-HIDE NAVBAR LOGIC
        // ==========================================
        if (mainNavWrapper) {
            let lastScrollY = window.scrollY;

            window.addEventListener('scroll', () => {
                // Calculate if user is within 200px of the absolute bottom of the page
                const isAtBottom = (window.innerHeight + window.scrollY) >= (document.body.offsetHeight - 200);

                if (isAtBottom) {
                    // Hide the navbar when touching the footer
                    mainNavWrapper.classList.add('-translate-y-[150%]', 'opacity-0');
                    if (dropdownMenu) dropdownMenu.classList.add('opacity-0', 'invisible', 'scale-95', 'translate-y-2');
                } 
                else if (window.scrollY <= 80) {
                    // Always show at the very top of the page
                    mainNavWrapper.classList.remove('-translate-y-[150%]', 'opacity-0');
                } 
                else if (window.scrollY > lastScrollY) {
                    // Hide the navbar when scrolling down
                    mainNavWrapper.classList.add('-translate-y-[150%]', 'opacity-0');
                    // Hide the dropdown if it was left open
                    if (dropdownMenu) dropdownMenu.classList.add('opacity-0', 'invisible', 'scale-95', 'translate-y-2');
                } 
                else {
                    // Show the navbar when scrolling up
                    mainNavWrapper.classList.remove('-translate-y-[150%]', 'opacity-0');
                }
                
                lastScrollY = window.scrollY;
            });
        }

        // INJECT FIREBASE SAFELY
        if (typeof firebase !== 'undefined' && firebase.auth) {
            this.setupAuth();
        } else {
            const script1 = document.createElement('script');
            script1.src = "https://www.gstatic.com/firebasejs/9.22.2/firebase-app-compat.js";
            document.head.appendChild(script1);
            
            script1.onload = () => {
                const script2 = document.createElement('script');
                script2.src = "https://www.gstatic.com/firebasejs/9.22.2/firebase-auth-compat.js";
                document.head.appendChild(script2);
                script2.onload = () => this.setupAuth();
            };
        }
    }

    setupAuth() {
        try {
            const firebaseConfig = {
                apiKey: "AIzaSyD8fonHbQIJyAuq9B7jFBnQi5lamIpjxfU",
                authDomain: "dxo-s-all.firebaseapp.com",
                projectId: "dxo-s-all",
                storageBucket: "dxo-s-all.firebasestorage.app",
                messagingSenderId: "208688244462",
                appId: "1:208688244462:web:5dd360753206ec129bf5ff"
            };
            
            if (!firebase.apps.length) { firebase.initializeApp(firebaseConfig); }
            const auth = firebase.auth();
            const provider = new firebase.auth.GoogleAuthProvider();

            const loginBtn = this.querySelector('#navLoginBtn');
            if (loginBtn) {
                loginBtn.addEventListener('click', () => { 
                    auth.signInWithPopup(provider)
                        .then(() => window.location.href = "dashboard.html")
                        .catch(err => {
                            console.error(err);
                            alert("Login failed! Did you enable Google Authentication in your Firebase Console?");
                        }); 
                });
            }
            
            const logoutBtn = this.querySelector('#navLogoutBtn');
            if (logoutBtn) {
                logoutBtn.addEventListener('click', () => { 
                    auth.signOut().then(() => window.location.href = "index.html"); 
                });
            }

            auth.onAuthStateChanged(user => {
            const outMenu = this.querySelector('#menu-logged-out');
            const inMenu = this.querySelector('#menu-logged-in');

            if (user) {
                if(outMenu) { outMenu.classList.add('hidden'); outMenu.classList.remove('block'); }
                if(inMenu) { inMenu.classList.remove('hidden'); inMenu.classList.add('block'); }
                
                this.querySelector('#nav-user-name').textContent = user.displayName ? user.displayName.split(' ')[0] : "User";
                this.querySelector('#dropdown-name').textContent = user.displayName || "Nexus User";
                this.querySelector('#dropdown-email').textContent = user.email;

                // ==========================================
                // FIX: SMART AVATAR HANDLING
                // ==========================================
                const defaultIcon = this.querySelector('#nav-default-icon');
                const pic = this.querySelector('#nav-profile-pic');
                
                if (user.photoURL && pic) {
                    // User has a photo (Google SSO) -> Show image, hide default SVG
                    pic.src = user.photoURL;
                    pic.classList.remove('hidden');
                    if (defaultIcon) defaultIcon.classList.add('hidden');
                } else {
                    // User has NO photo (Email Login) -> Hide image, show default SVG
                    if (pic) pic.classList.add('hidden');
                    if (defaultIcon) defaultIcon.classList.remove('hidden');
                }

            } else {
                if(outMenu) { outMenu.classList.remove('hidden'); outMenu.classList.add('block'); }
                if(inMenu) { inMenu.classList.add('hidden'); inMenu.classList.remove('block'); }
                
                this.querySelector('#nav-user-name').textContent = "Guest User";
                this.querySelector('#nav-default-icon').classList.remove('hidden');
                const pic = this.querySelector('#nav-profile-pic');
                if(pic) pic.classList.add('hidden');
            }
        });
        } catch(e) {
            console.error("Firebase setup error:", e);
        }
    }
}
customElements.define('nexus-navbar', NexusNavbar);