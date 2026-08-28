class NexusNavbar extends HTMLElement {
    connectedCallback() {
        this.innerHTML = `
        <style>
            .dropdown-menu { transform-origin: top right; transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1); }
            .dropdown-hidden { transform: translateY(10px) scale(0.95); opacity: 0; visibility: hidden; pointer-events: none; }
            #main-nav-wrapper { transition: top 0.4s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.4s ease; }
        </style>
        
        <!-- OUTER WRAPPER: Flex center guarantees it never drifts right on large screens -->
        <div id="main-nav-wrapper" class="fixed top-6 left-0 w-full z-50 flex justify-center">
            
            <!-- INNER WRAPPER: Handles the load animation independently -->
            <div class="w-[95%] max-w-[1200px] slide-up">
                <nav class="bg-zinc-900/80 backdrop-blur-xl border border-zinc-800 rounded-full px-6 h-16 flex items-center justify-between shadow-2xl">
                    
                    <!-- Brand -->
                    <a href="index.html" class="flex items-center gap-3 group">
                        <div class="w-8 h-8 rounded-full bg-gradient-to-tr from-amber-500 to-rose-500 flex items-center justify-center shadow-inner group-hover:scale-105 transition-transform">
                            <div class="w-3 h-3 bg-zinc-950 rounded-sm rotate-45"></div>
                        </div>
                        <span class="text-white font-bold tracking-wide text-lg">Nexus</span>
                    </a>

                    <!-- Links -->
                    <div class="hidden md:flex items-center gap-8 text-sm font-medium text-zinc-400">
                        <a href="platform.html" class="hover:text-amber-500 transition-colors">Platform</a>
                        <a href="solutions.html" class="hover:text-amber-500 transition-colors">Solutions</a>
                        <a href="documentation.html" class="hover:text-amber-500 transition-colors">Documentation</a>
                    </div>

                    <!-- Right Actions & Dropdown -->
                    <div class="flex items-center gap-4">
                        <div class="relative">
                            <button id="profileBtn" class="flex items-center gap-2 pl-2 pr-1 py-1 rounded-full bg-zinc-800 hover:bg-zinc-700 border border-zinc-700 transition-all focus:outline-none focus:ring-2 focus:ring-amber-500/50">
                                <span id="nav-user-name" class="text-xs font-semibold text-zinc-300 hidden sm:block">Guest User</span>
                                <div id="nav-avatar-container" class="w-8 h-8 rounded-full bg-zinc-900 flex items-center justify-center border border-zinc-600 overflow-hidden">
                                    <svg id="nav-default-icon" class="w-4 h-4 text-zinc-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                                    <img id="nav-profile-pic" src="" alt="Profile" class="w-full h-full object-cover hidden">
                                </div>
                            </button>
                            
                            <div id="dropdownMenu" class="dropdown-menu dropdown-hidden absolute right-0 mt-4 w-64 bg-zinc-900/95 backdrop-blur-xl border border-zinc-700/50 rounded-2xl shadow-[0_10px_40px_rgba(0,0,0,0.5)] p-2 z-[60]">
                                
                                <div id="menu-logged-out" class="py-1">
                                    <div class="px-3 py-2 mb-1"><p class="text-xs text-zinc-400">Sign in to access the engine</p></div>
                                    <button class="btn-google-login w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-zinc-300 hover:bg-white hover:text-zinc-900 transition-all">
                                        <svg class="w-4 h-4" viewBox="0 0 24 24"><path fill="currentColor" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/><path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/></svg>
                                        Login with Google
                                    </button>
                                </div>

                                <div id="menu-logged-in" class="hidden">
                                    <div class="px-3 py-3 border-b border-zinc-800 mb-1">
                                        <p id="dropdown-name" class="text-sm font-bold text-white">Loading...</p>
                                        <p id="dropdown-email" class="text-xs text-zinc-400 mt-0.5">loading...</p>
                                    </div>
                                    <div class="py-1">
                                        <a href="dashboard.html" class="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-zinc-300 hover:bg-amber-500/10 hover:text-amber-500 group transition-all">
                                            <div class="p-1.5 rounded-lg bg-zinc-800 group-hover:bg-amber-500/20 text-zinc-400 group-hover:text-amber-500 transition-colors">
                                                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path></svg>
                                            </div>
                                            Open CSPM Dashboard
                                        </a>
                                        <div class="border-t border-zinc-800/60 mt-1 pt-1">
                                            <button class="btn-logout w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-rose-400 hover:bg-rose-500/10 transition-all text-left">
                                                <div class="p-1.5 rounded-lg bg-rose-500/10 text-rose-400">
                                                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path></svg>
                                                </div>
                                                Log Out
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

        this.initLogic();
    }

    initLogic() {
        // Dropdown Toggle
        const profileBtn = this.querySelector('#profileBtn');
        const dropdownMenu = this.querySelector('#dropdownMenu');

        profileBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            dropdownMenu.classList.toggle('dropdown-hidden');
        });

        document.addEventListener('click', (e) => {
            if (!dropdownMenu.contains(e.target) && !profileBtn.contains(e.target)) {
                dropdownMenu.classList.add('dropdown-hidden');
            }
        });

        // Firebase Auth Integration (Waits for script to load)
        const checkFirebase = setInterval(() => {
            if (window.firebase && firebase.auth) {
                clearInterval(checkFirebase);
                const auth = firebase.auth();
                const provider = new firebase.auth.GoogleAuthProvider();

                // Login/Logout Handlers
                this.querySelectorAll('.btn-google-login').forEach(btn => {
                    btn.addEventListener('click', () => { auth.signInWithPopup(provider).catch(err => alert("Sign in failed.")); });
                });
                this.querySelectorAll('.btn-logout').forEach(btn => {
                    btn.addEventListener('click', () => { auth.signOut().then(() => dropdownMenu.classList.add('dropdown-hidden')); });
                });

                // UI State Listener
                auth.onAuthStateChanged(user => {
                    if (user) {
                        this.querySelector('#menu-logged-out').classList.add('hidden');
                        this.querySelector('#menu-logged-in').classList.remove('hidden');
                        this.querySelector('#nav-user-name').textContent = user.displayName.split(' ')[0];
                        this.querySelector('#nav-default-icon').classList.add('hidden');
                        this.querySelector('#nav-profile-pic').src = user.photoURL;
                        this.querySelector('#nav-profile-pic').classList.remove('hidden');
                        this.querySelector('#dropdown-name').textContent = user.displayName;
                        this.querySelector('#dropdown-email').textContent = user.email;
                    } else {
                        this.querySelector('#menu-logged-out').classList.remove('hidden');
                        this.querySelector('#menu-logged-in').classList.add('hidden');
                        this.querySelector('#nav-user-name').textContent = "Guest User";
                        this.querySelector('#nav-default-icon').classList.remove('hidden');
                        this.querySelector('#nav-profile-pic').classList.add('hidden');
                    }
                });
            }
        }, 100);

        // Auto Hide Scroll Logic
        const navWrapper = this.querySelector('#main-nav-wrapper');
        let lastScrollY = window.scrollY;

        window.addEventListener('scroll', () => {
            if (!navWrapper) return;
            const isAtBottom = (window.innerHeight + window.scrollY) >= (document.body.offsetHeight - 200);

            if (isAtBottom) {
                navWrapper.style.top = '-100px';
                navWrapper.style.opacity = '0';
            } else if (window.scrollY <= 80) {
                navWrapper.style.top = '1.5rem';
                navWrapper.style.opacity = '1';
            } else if (window.scrollY > lastScrollY) {
                navWrapper.style.top = '-100px';
                navWrapper.style.opacity = '0';
            } else {
                navWrapper.style.top = '1.5rem';
                navWrapper.style.opacity = '1';
            }
            lastScrollY = window.scrollY;
        });
    }
}

customElements.define('nexus-navbar', NexusNavbar);