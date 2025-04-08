<script lang="ts">
    import { Send } from "lucide-svelte";
	import { onNavigate } from "$app/navigation";
	import { Button } from "$lib/components/ui/button";
    import { Input } from "$lib/components/ui/input";
    import * as Card from "$lib/components/ui/card";
	import { onMount } from "svelte";

    import * as signalR from "@microsoft/signalr";
	import { Root } from "$lib/components/ui/accordion";
	import { getUserId } from "$lib/app.svelte";

    let currentGroup = $state("5263acae-75a0-4ed5-8f2a-095d4f82434a");
    let message = $state("");
    let messages: {
        authorId: string,
        authorName: string,
        content: string,
        sent: Date
    }[] = $state([]);

    let connection: signalR.HubConnection = $state(new signalR.HubConnectionBuilder()
        .withAutomaticReconnect()
        .withKeepAliveInterval(10000)
        .withUrl((window as any).platform.getServerUrl() + "/communication/hub", {
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets,
        })
        .build())

    onMount(beginSignalR);
    onNavigate(endSignalR);
    async function beginSignalR() {
        await connection.start()
    }
    async function endSignalR() {
        await connection.stop()
    }

    async function send() {
        await connection.send("SendMessage", getUserId() , currentGroup, message)
        messages = [...messages, {
            authorId: getUserId(),
            authorName: "Moi",
            content: message,
            sent: new Date()
        }];
        message = "";
    }

    async function createMessage(message: string) {
        messages = [...messages, {
            authorId: "" + Math.random(),
            authorName: "Someone" +  Math.round(Math.random()*100),
            content: message,
            sent: new Date()
        }];
    }
    (window as any).createMessage = createMessage;
</script>

<div class="flex-1">
    <Card.Root class="size-full flex flex-col">
        <Card.Header class="prose">
            <h2>Chat/{currentGroup}</h2>
        </Card.Header>
        <Card.Content class="flex flex-1 flex-col ">
            <div class="flex-1 bg-secondary overflow-y-scroll">
                {#each messages as m}
                <div class="flex flex-col">
                    <div class="flex {m.authorId == getUserId() ? "flex-row-reverse" : "flex-row"} w-full items-center" onsubmit={send}>
                        <div class="bg-primary text-center px-2 text-white">
                            {m.authorName}
                        </div>
                        <div class="w-full px-2 border border-black">
                            <p>{m.content}</p>
                        </div>
                    </div>
                    <div class="text-xs text-muted-foreground text-right">
                        {m.sent.toLocaleString()}
                    </div>
                </div>
                {/each}
            </div>
        </Card.Content>
        <Card.Footer>
            {#if currentGroup != ""}
            <form class="flex w-full items-center" onsubmit={send}>
                <Input type="text" class="" placeholder="Tapez votre message..." bind:value={message} />
                <Button type="submit">
                    Envoyer
                    <Send />
                </Button>
            </form>
            {/if}
        </Card.Footer>
    </Card.Root>
</div>