import { PubSubService, MessageContext } from "../../services/PubSub";
import * as MenuApi from "../../services/menu/MenuApi";
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { Route } from "vue-router";

interface IRouteNavigation {
    routeName: string;
    url: string;
    setMenu(name: String): void;
}
type NavigationCallback = (context: IRouteNavigation) => void;


@Component
export default class DiscoverMenuComponent extends Vue {
    private callbacks: NavigationCallback[] = [];

    childMenu: MenuApi.MenuItem[] = [];
    currentApplicationId: number | null = null;

    @Watch('$route.params.applicationId')
    onApplicationSelected(value: string, oldValue: string) {
        console.log('DiscoverMenuComponent.changed,', value, 'stored:', this.currentApplicationId, 'old', oldValue);
        if (!value) {
            this.currentApplicationId = null;
            return;
        }

        if (this.$route.fullPath.indexOf('/discover/') === -1) {
            return;
        }

        var applicationId = parseInt(value);
        this.currentApplicationId = applicationId;
    }

    created() {
        console.log('CREATED');
        PubSubService.Instance.subscribe(MenuApi.MessagingTopics.ApplicationChanged, this.onChanged);
    }

    mounted() {
        if (!this.$route.params.applicationId) {
            return;
        }

        var appId = parseInt(this.$route.params.applicationId);
        console.log('DiscoverMenuComponent.changed3,', appId, 'old', this.currentApplicationId);
        this.currentApplicationId = appId;
    }

    destroyed() {
        console.log('destroyed');
        PubSubService.Instance.unsubscribe(MenuApi.MessagingTopics.ApplicationChanged, this.onChanged);
    }

    testMe(e: any) {
        console.log('DiscoverMenuComponent.CLICK: ',e);
    }

    private onChanged(ctx: MessageContext) {
        console.log('DiscoverMenuComponent.OnChanged,', ctx, 'old', this.currentApplicationId);
        var msg = <MenuApi.SetApplication>ctx.message.body;
        this.currentApplicationId = msg.applicationId;
    }
}
