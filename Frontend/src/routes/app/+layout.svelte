<script lang="ts">
	import '../../app.css';
	import * as Sidebar from "$lib/components/ui/sidebar/index.js";
  	import AppSidebar from "$lib/components/app-sidebar.svelte";
	import * as Dialog from "$lib/components/ui/dialog";
	import { IsMobile } from '$lib/hooks/is-mobile.svelte';
	import { injectPlatform } from '$lib/platformInterface';
	import { getAppState } from '$lib/app.svelte';
	import { goto } from '$app/navigation';
	let { children } = $props();
	let isMobile = new IsMobile();
	injectPlatform();

	if(!getAppState().isLoggedIn) goto('/');

</script>

<Sidebar.Provider>
	<AppSidebar />
	<main class="p-4 w-full">
		{#if isMobile.current}
			<Sidebar.Trigger />
		{/if}
		{@render children()}
	</main>
</Sidebar.Provider>

