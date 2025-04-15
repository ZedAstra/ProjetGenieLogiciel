<script lang="ts">
	import { getBearerToken, getChantier, getClient } from "$lib/App.svelte";
	import { onMount } from "svelte";
    import { Button, Table, Tabs } from "svelte-ux";
	import { get } from "svelte/store";

    let options: {
        label: string;
        value: string;
    }[] = [
        {
            label: "Utilisateurs",
            value: "users",
        },
        {
            label: "Scalar",
            value: "scalar",
        },
        {
            label: "Outils",
            value: "tools",
        }
    ];
    let currentTab: string = $state("users");
    let scalar: HTMLIFrameElement | null = $state(null);

    let tableData: {
        Id: number;
        Nom: string;
        Prenom: string;
        Email: string;
        MotDePasse: string;
        Role: string;
    }[] = $state([]);

    onMount(async () => {
        await getClient().GET("/app/projects/{id}/members", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
                path: {
                    id: getChantier(),
                }
            },
        })
    })
</script>

<Tabs {options} bind:value={currentTab} classes={{ content: "h-[calc(100vh-84px)]"}}>
    <svelte:fragment slot="content" let:value >
        {#if currentTab == "users"}
            <Table data={tableData}
                columns={[
                    { name: "Id" },
                    { name: "Nom" },
                    { name: "Prenom" },
                    { name: "Email" },
                    { name: "MotDePasse" },
                    { name: "Role" },
                ]}>
            </Table>
        {:else if currentTab == "scalar"}
            <iframe title="scalar" id="scalar" src="https://localhost:7149/scalar" bind:this={scalar} width="100%" height="100%"></iframe>
        {/if}
        {#if currentTab == "tools"}
        <div class="size-full p-2">
            <Button variant="fill-light" color="secondary" onclick={() => {
                // Copy the bearer token to the clipboard
                const bearerToken = getBearerToken().split(" ")[1];
                if(!bearerToken) {
                    console.error("Bearer token not found");
                    return;
                }
                navigator.clipboard.writeText(bearerToken).then(() => {
                    alert("Token copié dans le presse-papier");
                }).catch(err => {
                    console.error("Failed to copy: ", err);
                });
            }}>Copier le token Jwt 🗒️</Button>
        </div>
        {/if}
    </svelte:fragment>
</Tabs>

