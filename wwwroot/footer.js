class NexusFooter extends HTMLElement {
    connectedCallback() {
        this.innerHTML = `
            <footer class="border-t border-zinc-800/60 bg-zinc-950/80 backdrop-blur-xl pt-12 pb-8 mt-20 w-full relative z-50">
                <div class="max-w-[1400px] mx-auto px-6">
                    <div class="grid grid-cols-1 md:grid-cols-4 gap-10 mb-10">
                        
                        <!-- Brand & Live Status -->
                        <div class="col-span-1 md:col-span-2">
                            <div class="flex items-center gap-3 mb-5">
                                <div class="w-7 h-7 rounded-full bg-gradient-to-tr from-amber-500 to-rose-500 flex items-center justify-center shadow-inner">
                                    <div class="w-2.5 h-2.5 bg-zinc-950 rounded-sm rotate-45"></div>
                                </div>
                                <span class="text-white font-bold tracking-wide text-lg">Nexus <span class="text-zinc-500 font-normal">CSPM</span></span>
                            </div>
                            <p class="text-sm text-zinc-400 mb-6 max-w-sm leading-relaxed">
                                Enterprise-grade Cloud Security Posture Management. Continuous telemetry and automated SOAR remediation for AWS, Azure, and GCP.
                            </p>
                            <!-- Real-time Status -->
                            <div class="inline-flex items-center gap-2.5 px-3 py-1.5 rounded-lg bg-emerald-500/10 border border-emerald-500/20">
                                <span class="relative flex h-2 w-2">
                                    <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
                                    <span class="relative inline-flex rounded-full h-2 w-2 bg-emerald-500"></span>
                                </span>
                                <span class="text-xs font-semibold text-emerald-400 uppercase tracking-wider">All Systems Operational</span>
                            </div>
                        </div>

                        <!-- Quick Links (UPDATED) -->
                        <div>
                            <h4 class="text-white font-bold mb-4 text-sm tracking-wide">Platform</h4>
                            <ul class="space-y-3 text-sm text-zinc-500 font-medium">
                                <li><a href="index.html" class="hover:text-amber-500 transition-colors flex items-center gap-2"><span class="w-1 h-1 rounded-full bg-zinc-700"></span> Home</a></li>
                                <li><a href="platform.html" class="hover:text-amber-500 transition-colors flex items-center gap-2"><span class="w-1 h-1 rounded-full bg-zinc-700"></span> Platform</a></li>
                                <li><a href="solutions.html" class="hover:text-amber-500 transition-colors flex items-center gap-2"><span class="w-1 h-1 rounded-full bg-zinc-700"></span> Solutions</a></li>
                                <li><a href="documentation.html" class="hover:text-amber-500 transition-colors flex items-center gap-2"><span class="w-1 h-1 rounded-full bg-zinc-700"></span> Documentation</a></li>
                                <li><a href="dashboard.html" class="hover:text-amber-500 transition-colors flex items-center gap-2"><span class="w-1 h-1 rounded-full bg-zinc-700"></span> Command Center</a></li>
                                <li><a href="https://github.com/AdithaGunawardhana/portify-web/blob/main/portify-web/frontend/app/page.tsx" target="_blank" class="hover:text-amber-500 transition-colors flex items-center gap-2"><span class="w-1 h-1 rounded-full bg-zinc-700"></span> GitHub Repo</a></li>
                            </ul>
                        </div>

                        <!-- Real-time Clock & Legal -->
                        <div>
                            <h4 class="text-white font-bold mb-4 text-sm tracking-wide">Live Telemetry (UTC)</h4>
                            <!-- THIS IS THE LIVE CLOCK -->
                            <div class="font-mono text-2xl font-bold text-amber-500 mb-6 tracking-widest bg-zinc-900/50 inline-block px-3 py-1.5 rounded-lg border border-amber-500/20 shadow-inner" id="nexus-live-clock">
                                00:00:00
                            </div>
                            <ul class="space-y-3 text-sm text-zinc-500 font-medium">
                                <li><a href="privacy.html" class="hover:text-white transition-colors">Privacy Policy</a></li>
                                <li><a href="terms.html" class="hover:text-white transition-colors">Terms of Service</a></li>
                            </ul>
                        </div>
                    </div>

                    <div class="flex flex-col md:flex-row items-center justify-between pt-6 border-t border-zinc-800/60 text-xs text-zinc-600 font-medium">
                        <p>&copy; <span id="nexus-year"></span> Nexus CSPM Engine. Built by DxO.</p>
                        <p class="mt-2 md:mt-0 flex items-center gap-1.5">
                            Built with <span class="text-zinc-400">ASP.NET Core</span> & <span class="text-sky-400 font-mono">Tailwind</span>
                        </p>
                    </div>
                </div>
            </footer>
        `;

        // Logic to make the clock tick in real-time
        this.updateClock();
        this.clockInterval = setInterval(() => this.updateClock(), 1000);

        // Logic to auto-update the copyright year
        document.getElementById('nexus-year').textContent = new Date().getFullYear();
    }

    // Stop the clock if the footer is ever removed from the page to save memory
    disconnectedCallback() {
        clearInterval(this.clockInterval);
    }

    // Format the time as HH:MM:SS
    updateClock() {
        const clockEl = document.getElementById('nexus-live-clock');
        if (clockEl) {
            const now = new Date();
            // Get UTC time and format it nicely
            const timeString = now.toISOString().substring(11, 19); 
            clockEl.textContent = timeString;
        }
    }
}

// Register the custom HTML tag!
customElements.define('nexus-footer', NexusFooter);