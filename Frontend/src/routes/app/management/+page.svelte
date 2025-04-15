<script lang="ts">
	import { getBearerToken, getClient, getMe } from "$lib/App.svelte";
	import type { components } from "$lib/v1";
	import { onMount } from "svelte";
	import { Button, Table, Tabs } from "svelte-ux";

    let membres: components["schemas"]["SafeUtilisateur"][] = $state([]);
    let tableData: {
        ID: number;
        Nom: string;
        Role: string;
    }[] = $state([]);
    let me = getMe();

    onMount(async () => {
        const { data, error } = await getClient().GET("/app/everyone", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
            },
        })
        if (error) {
            console.error(error);
            return;
        }
        if (data) {
            membres = data;
            tableData = membres.map((membre) => {
                return {
                    ID: membre.id!,
                    Nom: `${membre.firstName} ${membre.lastName}`,
                    Role: membre.userRole!,
                };
            });
            console.log(tableData);
        }
    })

    let options: {
        label: string;
        value: string;
    }[] = [
        {
            label: "Membres",
            value: "members",
        },
    ];
    let currentTab: string = $state("members");
</script>

<div>
    <Tabs {options} bind:value={currentTab}>
        <svelte:fragment slot="content" let:value>
            {#if currentTab == "members"}
                <Table data={tableData}
                columns={[
                    { name: "ID" },
                    { name: "Nom" },
                    { name: "Role" },
                ]}>
                </Table>
            {/if}
        </svelte:fragment>
        
    </Tabs>

</div>