<script lang="ts">
	import { goto } from '$app/navigation';
	import { getAppState } from '$lib/app.svelte';
	import { injectPlatform } from '$lib/platformInterface';
	import '../app.css';
	
	let { children } = $props();
	injectPlatform();

	function init() {
		//if(getAppState().isLoggedIn) goto('/app');
	}
	init();
</script>

{@render children()}

