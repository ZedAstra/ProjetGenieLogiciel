<script lang="ts">
	import { goto } from '$app/navigation';
	import { getMe, isLoggedIn } from '$lib/App.svelte';
	import { onMount } from 'svelte';
	import { Tabs } from 'svelte-ux';
	let { children } = $props();


	let options: {
		label: string,
		value: any,
	}[] = [
		{
			label: 'Accueil',
			value: '/app',
		},
		{
			label: "Planning",
			value: '/app/planning',
		},
		{
			label: "Communication",
			value: '/app/communication',
		},
		{
			label: 'Management',
			value: '/app/management',
		},
	];
	if(getMe()?.userRole == "Admin")
	{
		options.push({
			label: 'Admin',
			value: '/app/admin',
		});
	}
	let value = $state(window.location.pathname);

	$effect(() => {
		goto(value);
	});

	onMount(async () => {
		document.title = "App - " + getMe()?.firstName + " " + getMe()?.lastName + " - " + getMe()?.userRole;
		if((!isLoggedIn()))
		{
			goto('/');
		}
	})
</script>

<Tabs
    {options}
    placement="top"
    bind:value
    classes={{
      content: "border px-4 py-2 rounded-r min-h-[calc(100vh-34px)] overflow-y-auto",
      tab: { root: "rounded-l" },
    }}>
	<svelte:fragment slot="content">
		{@render children()}
	</svelte:fragment>
</Tabs>